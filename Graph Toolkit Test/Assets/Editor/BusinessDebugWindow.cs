using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class BusinessDebugWindow : EditorWindow
{
    private string baseUrl = "http://127.0.0.1:3000";
    private string playerId = "player";
    private float timeoutSeconds = 5f;
    private bool logRawResponse = true;
    private bool enableBusinessDebugLogs;
    private bool enableSimTickLogs;

    private string lotId = "lot_market_01";
    private string businessTypeId = "grocery_store";
    private string moduleId = "storage";
    private string supplierId = "supplier_wholesale_01";
    private string roleId = "cashier";
    private string contactId = "supplier_wholesale_01";
    private int markupPercent = 10;
    private int moneyAmount = 1000;
    private int stockAmount = 100;
    private int shelfAmount = 50;

    [MenuItem("Tools/Business/Debug")]
    private static void Open()
    {
        GetWindow<BusinessDebugWindow>("Business Debug");
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
        EditorGUILayout.LabelField("Debug Toggles", EditorStyles.boldLabel);
        enableBusinessDebugLogs = EditorGUILayout.Toggle("Business Debug Logs", enableBusinessDebugLogs);
        enableSimTickLogs = EditorGUILayout.Toggle("Simulation Tick Logs", enableSimTickLogs);
        ApplyDebugToggles();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Business Data", EditorStyles.boldLabel);
        lotId = EditorGUILayout.TextField("Lot Id", lotId);
        businessTypeId = EditorGUILayout.TextField("Business Type Id", businessTypeId);
        moduleId = EditorGUILayout.TextField("Module Id", moduleId);
        supplierId = EditorGUILayout.TextField("Supplier Id", supplierId);
        roleId = EditorGUILayout.TextField("Role Id", roleId);
        contactId = EditorGUILayout.TextField("Contact Id", contactId);
        markupPercent = EditorGUILayout.IntField("Markup %", markupPercent);
        moneyAmount = EditorGUILayout.IntField("Add Money", moneyAmount);
        stockAmount = EditorGUILayout.IntField("Storage Amount", stockAmount);
        shelfAmount = EditorGUILayout.IntField("Shelves Amount", shelfAmount);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Money"))
        {
            _ = SendActionAsync("add_money", JsonUtility.ToJson(new MoneyRequest
            {
                action = "add_money",
                playerId = ResolvePlayerId(),
                data = new MoneyData { amount = moneyAmount }
            }));
        }
        if (GUILayout.Button("Reset Current Player"))
        {
            ResetCurrentPlayer();
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Reset All Businesses"))
        {
            _ = SendActionAsync("reset_businesses", JsonUtility.ToJson(new ResetBusinessesRequest
            {
                action = "reset_businesses",
                playerId = ResolvePlayerId()
            }));
        }

        EditorGUILayout.Space(8);
        if (GUILayout.Button("Rent Business"))
        {
            _ = SendActionAsync("rent_business", JsonUtility.ToJson(new RentBusinessRequest
            {
                action = "rent_business",
                playerId = ResolvePlayerId(),
                data = new RentBusinessData { lotId = lotId }
            }));
        }

        if (GUILayout.Button("Assign Business Type"))
        {
            _ = SendActionAsync("assign_business_type", JsonUtility.ToJson(new AssignBusinessTypeRequest
            {
                action = "assign_business_type",
                playerId = ResolvePlayerId(),
                data = new AssignBusinessTypeData { lotId = lotId, businessTypeId = businessTypeId }
            }));
        }

        if (GUILayout.Button("Install Module"))
        {
            _ = SendActionAsync("install_business_module", JsonUtility.ToJson(new InstallModuleRequest
            {
                action = "install_business_module",
                playerId = ResolvePlayerId(),
                data = new InstallModuleData { lotId = lotId, moduleId = moduleId }
            }));
        }

        if (GUILayout.Button("Unlock Contact"))
        {
            _ = SendActionAsync("unlock_contact", JsonUtility.ToJson(new UnlockContactRequest
            {
                action = "unlock_contact",
                playerId = ResolvePlayerId(),
                data = new UnlockContactData { contactId = contactId }
            }));
        }

        if (GUILayout.Button("Assign Supplier"))
        {
            _ = SendActionAsync("assign_supplier", JsonUtility.ToJson(new AssignSupplierRequest
            {
                action = "assign_supplier",
                playerId = ResolvePlayerId(),
                data = new AssignSupplierData { lotId = lotId, supplierId = supplierId }
            }));
        }

        if (GUILayout.Button("Hire Worker"))
        {
            _ = SendActionAsync("hire_business_worker", JsonUtility.ToJson(new HireWorkerRequest
            {
                action = "hire_business_worker",
                playerId = ResolvePlayerId(),
                data = new HireWorkerData { lotId = lotId, roleId = roleId, contactId = contactId }
            }));
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Open Business"))
        {
            _ = SendActionAsync("open_business", JsonUtility.ToJson(new LotOnlyRequest
            {
                action = "open_business",
                playerId = ResolvePlayerId(),
                data = new LotOnlyData { lotId = lotId }
            }));
        }
        if (GUILayout.Button("Close Business"))
        {
            _ = SendActionAsync("close_business", JsonUtility.ToJson(new LotOnlyRequest
            {
                action = "close_business",
                playerId = ResolvePlayerId(),
                data = new LotOnlyData { lotId = lotId }
            }));
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Add Stock To Storage"))
        {
            _ = SendActionAsync("add_business_stock", JsonUtility.ToJson(new StockRequest
            {
                action = "add_business_stock",
                playerId = ResolvePlayerId(),
                data = new StockData { lotId = lotId, amount = stockAmount }
            }));
        }

        if (GUILayout.Button("Add Stock To Shelves"))
        {
            _ = SendActionAsync("add_business_shelf_stock", JsonUtility.ToJson(new StockRequest
            {
                action = "add_business_shelf_stock",
                playerId = ResolvePlayerId(),
                data = new StockData { lotId = lotId, amount = shelfAmount }
            }));
        }

        if (GUILayout.Button("Clear Stock"))
        {
            _ = SendActionAsync("clear_business_stock", JsonUtility.ToJson(new LotOnlyRequest
            {
                action = "clear_business_stock",
                playerId = ResolvePlayerId(),
                data = new LotOnlyData { lotId = lotId }
            }));
        }

        if (GUILayout.Button("Set Markup"))
        {
            _ = SendActionAsync("set_business_markup", JsonUtility.ToJson(new SetMarkupRequest
            {
                action = "set_business_markup",
                playerId = ResolvePlayerId(),
                data = new SetMarkupData { lotId = lotId, markupPercent = markupPercent }
            }));
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Fast Forward", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("10 min"))
        {
            SimulateSeconds(600f);
        }
        if (GUILayout.Button("1 hour"))
        {
            SimulateSeconds(3600f);
        }
        if (GUILayout.Button("1 day"))
        {
            SimulateSeconds(86400f);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Diagnostics", EditorStyles.boldLabel);
        if (GUILayout.Button("Print Business State"))
        {
            PrintBusinessState();
        }
        if (GUILayout.Button("Print Runtime Simulation State"))
        {
            PrintSimulationState();
        }
    }

    private void ApplyDebugToggles()
    {
        BusinessDebugLog.Enabled = enableBusinessDebugLogs;
        if (EditorApplication.isPlaying)
        {
            var bootstrap = FindObjectOfType<GameBootstrap>();
            if (bootstrap != null && bootstrap.BusinessSimulationService != null)
            {
                bootstrap.BusinessSimulationService.DebugLogTicks = enableSimTickLogs;
            }
        }
    }

    private string ResolvePlayerId()
    {
        return string.IsNullOrWhiteSpace(playerId) ? "player" : playerId;
    }

    private async Task SendActionAsync(string action, string json)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            Debug.LogWarning("[BusinessDebug] Base Url is empty.");
            return;
        }

        string url = $"{baseUrl.TrimEnd('/')}/api/action";
        Debug.Log($"[BusinessDebug] POST {url}\n{json}");

        using var request = new UnityWebRequest(url, "POST");
        byte[] body = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        if (timeoutSeconds >= 1f)
        {
            request.timeout = Mathf.CeilToInt(timeoutSeconds);
        }

        var start = DateTime.UtcNow;
        var op = request.SendWebRequest();
        while (!op.isDone)
        {
            if (timeoutSeconds > 0f && timeoutSeconds < 1f && (DateTime.UtcNow - start).TotalSeconds >= timeoutSeconds)
            {
                request.Abort();
                Debug.LogWarning("[BusinessDebug] Timeout");
                return;
            }
            await Task.Yield();
        }

        string responseText = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
        double elapsedMs = (DateTime.UtcNow - start).TotalMilliseconds;

        if (logRawResponse)
        {
            Debug.Log($"[BusinessDebug] Response ({request.responseCode}) in {elapsedMs:0.0}ms: {responseText}");
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[BusinessDebug] Network error: result={request.result}, error='{request.error}', elapsed={elapsedMs:0.0}ms");
            return;
        }

        try
        {
            var response = JsonUtility.FromJson<ActionResponse>(responseText);
            if (response != null)
            {
                Debug.Log($"[BusinessDebug] Result: success={response.success}, errorCode={response.errorCode}, message={response.message}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[BusinessDebug] Failed to parse response: {ex.Message}");
        }
    }

    private void ResetCurrentPlayer()
    {
        string root = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
        string filePath = Path.Combine(root, "server", "playerData", $"{ResolvePlayerId()}.json");
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"[BusinessDebug] Player file not found: {filePath}");
            return;
        }

        File.Delete(filePath);
        Debug.Log($"[BusinessDebug] Deleted player file: {filePath}");
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

    private void SimulateSeconds(float seconds)
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning("[BusinessDebug] Enter Play Mode to simulate.");
            return;
        }

        var bootstrap = FindObjectOfType<GameBootstrap>();
        if (bootstrap == null || bootstrap.BusinessSimulationService == null)
        {
            Debug.LogWarning("[BusinessDebug] BusinessSimulationService not found.");
            return;
        }

        bootstrap.BusinessSimulationService.SimulateSeconds(seconds);
    }

    private void PrintBusinessState()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning("[BusinessDebug] Enter Play Mode to print business state.");
            return;
        }

        var bootstrap = FindObjectOfType<GameBootstrap>();
        var state = bootstrap != null ? bootstrap.BusinessStateSyncService : null;
        if (state == null)
        {
            Debug.LogWarning("[BusinessDebug] BusinessStateSyncService not found.");
            return;
        }

        Debug.Log($"[BusinessDebug] Businesses={state.Businesses.Count} KnownContacts={state.KnownContacts.Count}");
        foreach (var business in state.GetAllBusinesses())
        {
            if (business == null) continue;
            Debug.Log($"[BusinessDebug] lotId='{business.lotId}' type='{business.businessTypeId}' open={business.isOpen} modules=[{string.Join(",", business.installedModules ?? new System.Collections.Generic.List<string>())}] storage={business.storageStock}/{business.storageCapacity} shelves={business.shelfStock}/{business.shelfCapacity}");
        }
    }

    private void PrintSimulationState()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning("[BusinessDebug] Enter Play Mode to print simulation state.");
            return;
        }

        var bootstrap = FindObjectOfType<GameBootstrap>();
        var sim = bootstrap != null ? bootstrap.BusinessSimulationService : null;
        if (sim == null)
        {
            Debug.LogWarning("[BusinessDebug] BusinessSimulationService not found.");
            return;
        }

        foreach (var state in sim.GetAllStates())
        {
            if (state == null) continue;
            Debug.Log($"[BusinessDebug] SIM lotId='{state.lotId}' storage={state.storageStock:0.##}/{state.storageCapacity} shelves={state.shelfStock:0.##}/{state.shelfCapacity} income={state.accumulatedIncome:0.##} expenses={state.accumulatedExpenses:0.##}");
        }
    }

    [Serializable]
    private class MoneyRequest
    {
        public string action;
        public string playerId;
        public MoneyData data;
    }

    [Serializable]
    private class MoneyData
    {
        public int amount;
    }

    [Serializable]
    private class RentBusinessRequest
    {
        public string action;
        public string playerId;
        public RentBusinessData data;
    }

    [Serializable]
    private class RentBusinessData
    {
        public string lotId;
    }

    [Serializable]
    private class AssignBusinessTypeRequest
    {
        public string action;
        public string playerId;
        public AssignBusinessTypeData data;
    }

    [Serializable]
    private class AssignBusinessTypeData
    {
        public string lotId;
        public string businessTypeId;
    }

    [Serializable]
    private class InstallModuleRequest
    {
        public string action;
        public string playerId;
        public InstallModuleData data;
    }

    [Serializable]
    private class InstallModuleData
    {
        public string lotId;
        public string moduleId;
    }

    [Serializable]
    private class UnlockContactRequest
    {
        public string action;
        public string playerId;
        public UnlockContactData data;
    }

    [Serializable]
    private class UnlockContactData
    {
        public string contactId;
    }

    [Serializable]
    private class AssignSupplierRequest
    {
        public string action;
        public string playerId;
        public AssignSupplierData data;
    }

    [Serializable]
    private class AssignSupplierData
    {
        public string lotId;
        public string supplierId;
    }

    [Serializable]
    private class HireWorkerRequest
    {
        public string action;
        public string playerId;
        public HireWorkerData data;
    }

    [Serializable]
    private class HireWorkerData
    {
        public string lotId;
        public string roleId;
        public string contactId;
    }

    [Serializable]
    private class LotOnlyRequest
    {
        public string action;
        public string playerId;
        public LotOnlyData data;
    }

    [Serializable]
    private class LotOnlyData
    {
        public string lotId;
    }

    [Serializable]
    private class StockRequest
    {
        public string action;
        public string playerId;
        public StockData data;
    }

    [Serializable]
    private class StockData
    {
        public string lotId;
        public int amount;
    }

    [Serializable]
    private class SetMarkupRequest
    {
        public string action;
        public string playerId;
        public SetMarkupData data;
    }

    [Serializable]
    private class SetMarkupData
    {
        public string lotId;
        public int markupPercent;
    }

    [Serializable]
    private class ResetBusinessesRequest
    {
        public string action;
        public string playerId;
    }

    [Serializable]
    private class ActionResponse
    {
        public bool success;
        public string errorCode;
        public string message;
    }
}
