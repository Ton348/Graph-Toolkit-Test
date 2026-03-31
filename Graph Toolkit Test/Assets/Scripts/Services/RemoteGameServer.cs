using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class RemoteGameServer : IGameServer
{
    private readonly string baseUrl;
    private readonly string playerId;
    private readonly float timeoutSeconds;
    private readonly bool debugLog;

    public RemoteGameServer(string baseUrl, string playerId, float timeoutSeconds, bool debugLog)
    {
        this.baseUrl = NormalizeBaseUrl(string.IsNullOrEmpty(baseUrl) ? "http://localhost:3000" : baseUrl);
        this.playerId = string.IsNullOrEmpty(playerId) ? "player" : playerId;
        if (timeoutSeconds <= 0f)
        {
            this.timeoutSeconds = 0f;
        }
        else
        {
            this.timeoutSeconds = Mathf.Clamp(timeoutSeconds, 0.1f, 120f);
        }
        this.debugLog = debugLog;
    }

    private string NormalizeBaseUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return "http://127.0.0.1:3000";
        }

        string trimmed = url.TrimEnd('/');
        try
        {
            var uri = new Uri(trimmed);
            if (string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                var builder = new UriBuilder(uri)
                {
                    Host = "127.0.0.1"
                };
                string normalized = builder.Uri.ToString().TrimEnd('/');
                if (debugLog)
                {
                    Debug.Log($"[RemoteGameServer] Normalized baseUrl '{trimmed}' -> '{normalized}'");
                }
                return normalized;
            }
        }
        catch
        {
        }

        return trimmed;
    }

    public Task<ServerActionResult> TryGetProfileAsync()
    {
        if (debugLog)
        {
            Debug.Log("[RemoteGameServer] action=get_profile");
        }
        var request = new RemoteProfileRequest
        {
            action = "get_profile",
            playerId = playerId
        };

        return SendRequestAsync(request);
    }

    public Task<ServerActionResult> TryBuyBuildingAsync(string buildingId, QuestActionType questAction = QuestActionType.None, string questId = null)
    {
        if (debugLog)
        {
            Debug.Log($"[RemoteGameServer] action=buy_building buildingId='{buildingId}' questAction='{questAction}' questId='{questId}'");
        }
        var request = new RemoteBuyBuildingRequest
        {
            action = "buy_building",
            playerId = playerId,
            data = new RemoteBuyBuildingData
            {
                buildingId = buildingId,
                questAction = MapQuestAction(questAction),
                questId = questId
            }
        };

        return SendRequestAsync(request);
    }

    public Task<ServerActionResult> TryStartQuestAsync(string questId)
    {
        if (debugLog)
        {
            Debug.Log($"[RemoteGameServer] action=start_quest questId='{questId}'");
        }
        var request = new RemoteQuestRequest
        {
            action = "start_quest",
            playerId = playerId,
            data = new RemoteQuestData { questId = questId }
        };

        return SendRequestAsync(request);
    }

    public Task<ServerActionResult> TryCompleteQuestAsync(string questId)
    {
        if (debugLog)
        {
            Debug.Log($"[RemoteGameServer] action=complete_quest questId='{questId}'");
        }
        var request = new RemoteQuestRequest
        {
            action = "complete_quest",
            playerId = playerId,
            data = new RemoteQuestData { questId = questId }
        };

        return SendRequestAsync(request);
    }

    public Task<ServerActionResult> TryFailQuestAsync(string questId)
    {
        if (debugLog)
        {
            Debug.Log($"[RemoteGameServer] action=fail_quest questId='{questId}'");
        }
        var request = new RemoteQuestRequest
        {
            action = "fail_quest",
            playerId = playerId,
            data = new RemoteQuestData { questId = questId }
        };

        return SendRequestAsync(request);
    }

    public Task<ServerActionResult> TryAddMoneyAsync(int amount)
    {
        if (debugLog)
        {
            Debug.Log($"[RemoteGameServer] action=add_money amount={amount}");
        }
        var request = new RemoteMoneyRequest
        {
            action = "add_money",
            playerId = playerId,
            data = new RemoteMoneyData { amount = amount }
        };

        return SendRequestAsync(request);
    }

    public Task<ServerActionResult> TrySpendMoneyAsync(int amount)
    {
        if (debugLog)
        {
            Debug.Log($"[RemoteGameServer] action=spend_money amount={amount}");
        }
        var request = new RemoteMoneyRequest
        {
            action = "spend_money",
            playerId = playerId,
            data = new RemoteMoneyData { amount = amount }
        };

        return SendRequestAsync(request);
    }

    public Task<ServerActionResult> TryStealAsync(int amount, bool canFail, int successChance)
    {
        if (debugLog)
        {
            Debug.Log($"[RemoteGameServer] action=steal amount={amount} canFail={canFail} successChance={successChance}");
        }
        var request = new RemoteStealRequest
        {
            action = "steal",
            playerId = playerId,
            data = new RemoteStealData { amount = amount, canFail = canFail, successChance = successChance }
        };

        return SendRequestAsync(request);
    }

    private async Task<ServerActionResult> SendRequestAsync<T>(T requestPayload)
    {
        string url = $"{baseUrl}/api/action";
        string payload = JsonUtility.ToJson(requestPayload);

        if (debugLog)
        {
            Debug.Log($"[RemoteGameServer] POST {url}\n{payload}");
        }

        using var request = new UnityWebRequest(url, "POST");
        byte[] body = Encoding.UTF8.GetBytes(payload);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        if (timeoutSeconds >= 1f)
        {
            request.timeout = Mathf.CeilToInt(timeoutSeconds);
        }

        bool manualTimeout = false;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var op = request.SendWebRequest();
        while (!op.isDone)
        {
            if (timeoutSeconds > 0f && timeoutSeconds < 1f && stopwatch.Elapsed.TotalSeconds >= timeoutSeconds)
            {
                manualTimeout = true;
                request.Abort();
                break;
            }
            await Task.Yield();
        }

        double elapsedMs = stopwatch.Elapsed.TotalMilliseconds;

        if (manualTimeout)
        {
            if (debugLog)
            {
                string timeoutLabel = timeoutSeconds > 0f ? $"{timeoutSeconds:0.###}s" : "disabled";
                Debug.LogWarning($"[RemoteGameServer] Network error: result=Timeout, error='Request timeout', url={url}, timeout={timeoutLabel}, elapsed={elapsedMs:0.0}ms");
            }
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.Timeout, "Timeout", "Request timeout.");
        }

        string responseText = request.downloadHandler != null ? request.downloadHandler.text : null;
        if (debugLog)
        {
            Debug.Log($"[RemoteGameServer] Response ({request.responseCode}) in {elapsedMs:0.0}ms: {responseText}");
            if (request.result != UnityWebRequest.Result.Success)
            {
                string timeoutLabel = timeoutSeconds > 0f ? $"{timeoutSeconds:0.###}s" : "disabled";
                Debug.LogWarning($"[RemoteGameServer] Network error: result={request.result}, error='{request.error}', url={url}, timeout={timeoutLabel}, elapsed={elapsedMs:0.0}ms");
            }
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            ServerActionResult.ErrorType errorType = MapNetworkError(request);
            string errorCode = $"Http{request.responseCode}";
            string message = request.error;
            return ServerActionResult.FailResult(errorType, errorCode, message);
        }

        if (string.IsNullOrEmpty(responseText))
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.NetworkError, "EmptyResponse", "Server returned empty response.");
        }

        RemoteActionResponse response = null;
        try
        {
            response = JsonUtility.FromJson<RemoteActionResponse>(responseText);
        }
        catch (Exception ex)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.NetworkError, "InvalidJson", ex.Message);
        }

        if (response == null)
        {
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.NetworkError, "InvalidResponse", "Response could not be parsed.");
        }

        ProfileSnapshot snapshot = response.profile != null ? MapProfileSnapshot(response.profile) : null;

        if (!response.success)
        {
            if (debugLog)
            {
                Debug.Log($"[RemoteGameServer] Result: success=false errorCode={response.errorCode} message={response.message}");
            }
            return ServerActionResult.FailResult(ServerActionResult.ErrorType.GameLogicError, response.errorCode ?? "ServerError", response.message);
        }

        if (debugLog)
        {
            Debug.Log($"[RemoteGameServer] Result: success=true message={response.message}");
        }
        return ServerActionResult.SuccessResult(snapshot, response.message);
    }

    private ServerActionResult.ErrorType MapNetworkError(UnityWebRequest request)
    {
        if (request == null)
        {
            return ServerActionResult.ErrorType.NetworkError;
        }

        string error = request.error ?? string.Empty;
        if (error.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return ServerActionResult.ErrorType.Timeout;
        }

        if (request.responseCode == 408 || request.responseCode == 504)
        {
            return ServerActionResult.ErrorType.Timeout;
        }

        return ServerActionResult.ErrorType.NetworkError;
    }

    private ProfileSnapshot MapProfileSnapshot(RemoteProfileDto profile)
    {
        var snapshot = new ProfileSnapshot
        {
            Money = profile.money,
            Bargaining = profile.bargaining,
            Speech = profile.speech,
            Speed = profile.speed,
            Damage = profile.damage,
            Health = profile.health
        };

        if (profile.activeQuests != null)
        {
            snapshot.ActiveQuestIds.AddRange(profile.activeQuests);
        }

        if (profile.completedQuests != null)
        {
            snapshot.CompletedQuestIds.AddRange(profile.completedQuests);
        }

        if (profile.buildings != null)
        {
            snapshot.OwnedBuildingIds.AddRange(profile.buildings);
        }

        if (profile.buildingStates != null)
        {
            foreach (var state in profile.buildingStates)
            {
                if (state == null || string.IsNullOrEmpty(state.id))
                {
                    continue;
                }

                snapshot.BuildingStates.Add(new BuildingStateSnapshot
                {
                    id = state.id,
                    owned = state.owned,
                    level = state.level,
                    currentIncome = state.currentIncome,
                    currentExpenses = state.currentExpenses
                });
            }
        }

        return snapshot;
    }

    [Serializable]
    private class RemoteProfileRequest
    {
        public string action;
        public string playerId;
    }

    [Serializable]
    private class RemoteBuyBuildingRequest
    {
        public string action;
        public string playerId;
        public RemoteBuyBuildingData data;
    }

    [Serializable]
    private class RemoteBuyBuildingData
    {
        public string buildingId;
        public string questAction;
        public string questId;
    }

    [Serializable]
    private class RemoteQuestRequest
    {
        public string action;
        public string playerId;
        public RemoteQuestData data;
    }

    [Serializable]
    private class RemoteQuestData
    {
        public string questId;
    }

    [Serializable]
    private class RemoteMoneyRequest
    {
        public string action;
        public string playerId;
        public RemoteMoneyData data;
    }

    [Serializable]
    private class RemoteMoneyData
    {
        public int amount;
    }

    [Serializable]
    private class RemoteStealRequest
    {
        public string action;
        public string playerId;
        public RemoteStealData data;
    }

    [Serializable]
    private class RemoteStealData
    {
        public int amount;
        public bool canFail;
        public int successChance;
    }

    [Serializable]
    private class RemoteActionResponse
    {
        public bool success;
        public string errorCode;
        public string message;
        public RemoteProfileDto profile;
    }

    [Serializable]
    private class RemoteProfileDto
    {
        public int money;
        public string[] activeQuests;
        public string[] completedQuests;
        public string[] buildings;
        public int bargaining;
        public int speech;
        public int speed;
        public int damage;
        public int health;
        public RemoteBuildingStateDto[] buildingStates;
    }

    [Serializable]
    private class RemoteBuildingStateDto
    {
        public string id;
        public bool owned;
        public int level;
        public int currentIncome;
        public int currentExpenses;
    }

    private static string MapQuestAction(QuestActionType action)
    {
        return action switch
        {
            QuestActionType.StartQuest => "start",
            QuestActionType.CompleteQuest => "complete",
            _ => null
        };
    }
}
