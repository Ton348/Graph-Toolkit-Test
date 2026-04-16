using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Prototype.Business.Bootstrap;
using Prototype.Business.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class BusinessDebugWindow : EditorWindow
{
    private string m_baseUrl = "http://127.0.0.1:3000";
    private string m_playerId = "player";
    private float m_timeoutSeconds = 5f;
    private bool m_logRawResponse = true;
    private bool m_enableBusinessDebugLogs;
    private bool m_enableSimTickLogs;

    private string m_lotId = "lot_market_01";
    private string m_businessTypeId = "grocery_store";
    private string m_moduleId = "storage";
    private string m_supplierId = "supplier_wholesale_01";
    private string m_roleId = "cashier";
    private string m_contactId = "supplier_wholesale_01";
    private int m_markupPercent = 10;
    private int m_moneyAmount = 1000;
    private int m_stockAmount = 100;
    private int m_shelfAmount = 50;

    [MenuItem("Tools/Business/Debug")]
    private static void Open()
    {
        GetWindow<BusinessDebugWindow>("Business Debug");
    }

    private void OnEnable()
    {
        TryLoadFromBootstrap();
    }

    private void OnGui()
    {
        EditorGUILayout.LabelField("Server", EditorStyles.boldLabel);
        m_baseUrl = EditorGUILayout.TextField("Base Url", m_baseUrl);
        m_playerId = EditorGUILayout.TextField("Player Id", m_playerId);
        m_timeoutSeconds = EditorGUILayout.FloatField("Timeout (sec)", m_timeoutSeconds);
        m_logRawResponse = EditorGUILayout.Toggle("Log Raw Response", m_logRawResponse);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Debug Toggles", EditorStyles.boldLabel);
        m_enableBusinessDebugLogs = EditorGUILayout.Toggle("Business Debug Logs", m_enableBusinessDebugLogs);
        m_enableSimTickLogs = EditorGUILayout.Toggle("Simulation Tick Logs", m_enableSimTickLogs);
        ApplyDebugToggles();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Business Data", EditorStyles.boldLabel);
        m_lotId = EditorGUILayout.TextField("Lot Id", m_lotId);
        m_businessTypeId = EditorGUILayout.TextField("Business Type Id", m_businessTypeId);
        m_moduleId = EditorGUILayout.TextField("Module Id", m_moduleId);
        m_supplierId = EditorGUILayout.TextField("Supplier Id", m_supplierId);
        m_roleId = EditorGUILayout.TextField("Role Id", m_roleId);
        m_contactId = EditorGUILayout.TextField("Contact Id", m_contactId);
        m_markupPercent = EditorGUILayout.IntField("Markup %", m_markupPercent);
        m_moneyAmount = EditorGUILayout.IntField("Add Money", m_moneyAmount);
        m_stockAmount = EditorGUILayout.IntField("Storage Amount", m_stockAmount);
        m_shelfAmount = EditorGUILayout.IntField("Shelves Amount", m_shelfAmount);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Money"))
        {
            _ = SendActionAsync("add_money", JsonUtility.ToJson(new MoneyRequest
            {
                action = "add_money",
                playerId = ResolvePlayerId(),
                data = new MoneyData { amount = m_moneyAmount }
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
                data = new RentBusinessData { lotId = m_lotId }
            }));
        }

        if (GUILayout.Button("Assign Business Type"))
        {
            _ = SendActionAsync("assign_business_type", JsonUtility.ToJson(new AssignBusinessTypeRequest
            {
                action = "assign_business_type",
                playerId = ResolvePlayerId(),
                data = new AssignBusinessTypeData { lotId = m_lotId, businessTypeId = m_businessTypeId }
            }));
        }

        if (GUILayout.Button("Install Module"))
        {
            _ = SendActionAsync("install_business_module", JsonUtility.ToJson(new InstallModuleRequest
            {
                action = "install_business_module",
                playerId = ResolvePlayerId(),
                data = new InstallModuleData { lotId = m_lotId, moduleId = m_moduleId }
            }));
        }

        if (GUILayout.Button("Unlock Contact"))
        {
            _ = SendActionAsync("unlock_contact", JsonUtility.ToJson(new UnlockContactRequest
            {
                action = "unlock_contact",
                playerId = ResolvePlayerId(),
                data = new UnlockContactData { contactId = m_contactId }
            }));
        }

        if (GUILayout.Button("Assign Supplier"))
        {
            _ = SendActionAsync("assign_supplier", JsonUtility.ToJson(new AssignSupplierRequest
            {
                action = "assign_supplier",
                playerId = ResolvePlayerId(),
                data = new AssignSupplierData { lotId = m_lotId, supplierId = m_supplierId }
            }));
        }

        if (GUILayout.Button("Hire Worker"))
        {
            _ = SendActionAsync("hire_business_worker", JsonUtility.ToJson(new HireWorkerRequest
            {
                action = "hire_business_worker",
                playerId = ResolvePlayerId(),
                data = new HireWorkerData { lotId = m_lotId, roleId = m_roleId, contactId = m_contactId }
            }));
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Open Business"))
        {
            _ = SendActionAsync("open_business", JsonUtility.ToJson(new LotOnlyRequest
            {
                action = "open_business",
                playerId = ResolvePlayerId(),
                data = new LotOnlyData { lotId = m_lotId }
            }));
        }
        if (GUILayout.Button("Close Business"))
        {
            _ = SendActionAsync("close_business", JsonUtility.ToJson(new LotOnlyRequest
            {
                action = "close_business",
                playerId = ResolvePlayerId(),
                data = new LotOnlyData { lotId = m_lotId }
            }));
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Add Stock To Storage"))
        {
            _ = SendActionAsync("add_business_stock", JsonUtility.ToJson(new StockRequest
            {
                action = "add_business_stock",
                playerId = ResolvePlayerId(),
                data = new StockData { lotId = m_lotId, amount = m_stockAmount }
            }));
        }

        if (GUILayout.Button("Add Stock To Shelves"))
        {
            _ = SendActionAsync("add_business_shelf_stock", JsonUtility.ToJson(new StockRequest
            {
                action = "add_business_shelf_stock",
                playerId = ResolvePlayerId(),
                data = new StockData { lotId = m_lotId, amount = m_shelfAmount }
            }));
        }

        if (GUILayout.Button("Clear Stock"))
        {
            _ = SendActionAsync("clear_business_stock", JsonUtility.ToJson(new LotOnlyRequest
            {
                action = "clear_business_stock",
                playerId = ResolvePlayerId(),
                data = new LotOnlyData { lotId = m_lotId }
            }));
        }

        if (GUILayout.Button("Set Markup"))
        {
            _ = SendActionAsync("set_business_markup", JsonUtility.ToJson(new SetMarkupRequest
            {
                action = "set_business_markup",
                playerId = ResolvePlayerId(),
                data = new SetMarkupData { lotId = m_lotId, markupPercent = m_markupPercent }
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
        BusinessDebugLog.enabled = m_enableBusinessDebugLogs;
        if (EditorApplication.isPlaying)
        {
            var bootstrap = FindObjectOfType<GameBootstrap>();
            if (bootstrap != null && bootstrap.BusinessSimulationService != null)
            {
                bootstrap.BusinessSimulationService.DebugLogTicks = m_enableSimTickLogs;
            }
        }
    }

    private string ResolvePlayerId()
    {
        return string.IsNullOrWhiteSpace(m_playerId) ? "player" : m_playerId;
    }

    private async Task SendActionAsync(string action, string json)
    {
        if (string.IsNullOrWhiteSpace(m_baseUrl))
        {
            Debug.LogWarning("[BusinessDebug] Base Url is empty.");
            return;
        }

        string url = $"{m_baseUrl.TrimEnd('/')}/api/action";
        Debug.Log($"[BusinessDebug] POST {url}\n{json}");

        using var request = new UnityWebRequest(url, "POST");
        byte[] body = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        if (m_timeoutSeconds >= 1f)
        {
            request.timeout = Mathf.CeilToInt(m_timeoutSeconds);
        }

        var start = DateTime.UtcNow;
        var op = request.SendWebRequest();
        while (!op.isDone)
        {
            if (m_timeoutSeconds > 0f && m_timeoutSeconds < 1f && (DateTime.UtcNow - start).TotalSeconds >= m_timeoutSeconds)
            {
                request.Abort();
                Debug.LogWarning("[BusinessDebug] Timeout");
                return;
            }
            await Task.Yield();
        }

        string responseText = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
        double elapsedMs = (DateTime.UtcNow - start).TotalMilliseconds;

        if (m_logRawResponse)
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

        m_baseUrl = bootstrap.remoteBaseUrl;
        m_playerId = bootstrap.remotePlayerId;
        m_timeoutSeconds = bootstrap.remoteTimeoutSeconds;
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
