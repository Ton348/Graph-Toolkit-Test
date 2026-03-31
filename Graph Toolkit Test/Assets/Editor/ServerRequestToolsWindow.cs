using System;
using Stopwatch = System.Diagnostics.Stopwatch;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class ServerRequestToolsWindow : EditorWindow
{
    private string baseUrl = "http://127.0.0.1:3000";
    private string playerId = "player";
    private string questId = "buy_building";
    private string buildingId = "car_wash_01";
    private float timeoutSeconds = 5f;
    private bool logRawResponse = true;

    [MenuItem("Tools/Server Requests/Quick Actions")]
    private static void Open()
    {
        GetWindow<ServerRequestToolsWindow>("Server Requests");
    }

    private void OnEnable()
    {
        TryLoadFromBootstrap();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Server", EditorStyles.boldLabel);
        baseUrl = EditorGUILayout.TextField("Base Url", baseUrl);
        playerId = EditorGUILayout.TextField("Player Id", playerId);
        timeoutSeconds = EditorGUILayout.FloatField("Timeout (sec)", timeoutSeconds);
        logRawResponse = EditorGUILayout.Toggle("Log Raw Response", logRawResponse);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Ids", EditorStyles.boldLabel);
        questId = EditorGUILayout.TextField("Quest Id", questId);
        buildingId = EditorGUILayout.TextField("Building Id", buildingId);

        EditorGUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Get Profile"))
        {
            _ = SendActionAsync("get_profile");
        }
        if (GUILayout.Button("Start Quest"))
        {
            _ = SendActionAsync("start_quest");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Complete Quest"))
        {
            _ = SendActionAsync("complete_quest");
        }
        if (GUILayout.Button("Buy Building"))
        {
            _ = SendActionAsync("buy_building");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);
        if (GUILayout.Button("Reset Current Player"))
        {
            ResetCurrentPlayer();
        }
    }

    private async Task SendActionAsync(string action)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            Debug.LogWarning("[ServerTool] Base Url is empty.");
            return;
        }

        string resolvedPlayerId = string.IsNullOrWhiteSpace(playerId) ? "player" : playerId;
        string json;

        switch (action)
        {
            case "get_profile":
                json = JsonUtility.ToJson(new GetProfileRequest
                {
                    action = action,
                    playerId = resolvedPlayerId
                });
                break;
            case "start_quest":
                json = JsonUtility.ToJson(new StartQuestRequest
                {
                    action = action,
                    playerId = resolvedPlayerId,
                    data = new QuestData { questId = questId }
                });
                break;
            case "complete_quest":
                json = JsonUtility.ToJson(new CompleteQuestRequest
                {
                    action = action,
                    playerId = resolvedPlayerId,
                    data = new QuestData { questId = questId }
                });
                break;
            case "buy_building":
                json = JsonUtility.ToJson(new BuyBuildingRequest
                {
                    action = action,
                    playerId = resolvedPlayerId,
                    data = new BuyBuildingData { buildingId = buildingId }
                });
                break;
            default:
                Debug.LogError($"[ServerTool] Unknown action: {action}");
                return;
        }

        string url = $"{baseUrl.TrimEnd('/')}/api/action";
        if (action == "start_quest" || action == "complete_quest")
        {
            Debug.Log($"[ServerTool] action={action} questId='{questId}'");
        }
        else if (action == "buy_building")
        {
            Debug.Log($"[ServerTool] action={action} buildingId='{buildingId}'");
        }

        Debug.Log($"[ServerTool] POST {url}\n{json}");

        using var request = new UnityWebRequest(url, "POST");
        byte[] body = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        if (timeoutSeconds >= 1f)
        {
            request.timeout = Mathf.CeilToInt(timeoutSeconds);
        }

        var stopwatch = Stopwatch.StartNew();
        var op = request.SendWebRequest();
        while (!op.isDone)
        {
            if (timeoutSeconds > 0f && timeoutSeconds < 1f && stopwatch.Elapsed.TotalSeconds >= timeoutSeconds)
            {
                request.Abort();
                Debug.LogWarning($"[ServerTool] Timeout after {stopwatch.Elapsed.TotalMilliseconds:0.0}ms");
                return;
            }
            await Task.Yield();
        }

        string responseText = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
        double elapsedMs = stopwatch.Elapsed.TotalMilliseconds;

        if (logRawResponse)
        {
            Debug.Log($"[ServerTool] Response ({request.responseCode}) in {elapsedMs:0.0}ms: {responseText}");
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[ServerTool] Network error: result={request.result}, error='{request.error}', url={url}, elapsed={elapsedMs:0.0}ms");
            return;
        }

        try
        {
            var response = JsonUtility.FromJson<ActionResponse>(responseText);
            if (response != null)
            {
                Debug.Log($"[ServerTool] Result: success={response.success}, errorCode={response.errorCode}, message={response.message}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[ServerTool] Failed to parse response: {ex.Message}");
        }
    }

    private void ResetCurrentPlayer()
    {
        string root = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
        string filePath = Path.Combine(root, "server", "playerData", $"{playerId}.json");
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"[ServerTool] Player file not found: {filePath}");
            return;
        }

        File.Delete(filePath);
        Debug.Log($"[ServerTool] Deleted player file: {filePath}");
    }

    private void TryLoadFromBootstrap()
    {
        var bootstrap = FindObjectOfType<GameBootstrap>();
        if (bootstrap == null)
        {
            return;
        }

        baseUrl = bootstrap.remoteBaseUrl;
        playerId = bootstrap.remotePlayerId;
        timeoutSeconds = bootstrap.remoteTimeoutSeconds;
    }

    [Serializable]
    private class GetProfileRequest
    {
        public string action;
        public string playerId;
    }

    [Serializable]
    private class StartQuestRequest
    {
        public string action;
        public string playerId;
        public QuestData data;
    }

    [Serializable]
    private class CompleteQuestRequest
    {
        public string action;
        public string playerId;
        public QuestData data;
    }

    [Serializable]
    private class BuyBuildingRequest
    {
        public string action;
        public string playerId;
        public BuyBuildingData data;
    }

    [Serializable]
    private class QuestData
    {
        public string questId;
    }

    [Serializable]
    private class BuyBuildingData
    {
        public string buildingId;
    }

    [Serializable]
    private class ActionResponse
    {
        public bool success;
        public string errorCode;
        public string message;
    }
}
