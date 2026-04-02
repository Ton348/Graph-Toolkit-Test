using UnityEngine;

public class BusinessModuleSlot : MonoBehaviour
{
    public string moduleId;
    public BusinessWorldRuntime worldRuntime;
    public Transform moduleVisual;
    public KeyCode interactKey = KeyCode.E;
    public string playerTag = "Player";

    private bool isInstalled;
    private BusinessStateSyncService stateSync;

    private void OnEnable()
    {
        ResolveStateSync();
        RefreshFromState();
        if (stateSync != null)
        {
            stateSync.StateChanged += RefreshFromState;
        }
    }

    private void OnDisable()
    {
        if (stateSync != null)
        {
            stateSync.StateChanged -= RefreshFromState;
        }
    }

    private void ResolveStateSync()
    {
        if (worldRuntime == null)
        {
            worldRuntime = GetComponentInParent<BusinessWorldRuntime>();
        }

        stateSync = worldRuntime != null && worldRuntime.bootstrap != null
            ? worldRuntime.bootstrap.BusinessStateSyncService
            : null;
    }

    private void Start()
    {
        RefreshFromState();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        if (!Input.GetKeyDown(interactKey))
        {
            return;
        }

        var carrier = other.GetComponentInParent<PlayerCarryItem>() ?? other.GetComponent<PlayerCarryItem>();
        if (carrier == null)
        {
            return;
        }

        TryInstall(carrier);
    }

    private async void TryInstall(PlayerCarryItem carrier)
    {
        if (isInstalled)
        {
            BusinessDebugLog.Log($"[BusinessWorld] Module already installed '{moduleId}' lotId='{worldRuntime?.lotId}'.");
            return;
        }

        if (worldRuntime == null)
        {
            worldRuntime = GetComponentInParent<BusinessWorldRuntime>();
        }

        if (worldRuntime == null || string.IsNullOrWhiteSpace(moduleId))
        {
            BusinessDebugLog.Warn("[BusinessWorld] Module slot missing runtime or moduleId.");
            return;
        }

        if (!carrier.HasItem(moduleId))
        {
            BusinessDebugLog.Log($"[BusinessWorld] Player missing item '{moduleId}' for lotId='{worldRuntime.lotId}'.");
            return;
        }

        var facade = worldRuntime.GetActionFacade();
        if (facade == null)
        {
            BusinessDebugLog.Warn("[BusinessWorld] BusinessActionFacade missing.");
            return;
        }

        BusinessDebugLog.Log($"[BusinessWorld] Install module '{moduleId}' lotId='{worldRuntime.lotId}'");
        var result = await facade.InstallModule(worldRuntime.lotId, moduleId);
        if (result != null && result.Success)
        {
            carrier.TryConsume(moduleId);
            isInstalled = true;
            SetVisual(true);
            BusinessDebugLog.Log($"[BusinessWorld] Module installed '{moduleId}' lotId='{worldRuntime.lotId}'");
        }
        else
        {
            BusinessDebugLog.Warn($"[BusinessWorld] Module install failed '{moduleId}' lotId='{worldRuntime.lotId}'");
        }
    }

    public void RefreshFromState()
    {
        if (worldRuntime == null)
        {
            worldRuntime = GetComponentInParent<BusinessWorldRuntime>();
        }

        var business = worldRuntime != null ? worldRuntime.GetBusiness() : null;
        isInstalled = business != null && business.installedModules != null && business.installedModules.Contains(moduleId);
        SetVisual(isInstalled);
    }

    private void SetVisual(bool active)
    {
        if (moduleVisual != null)
        {
            moduleVisual.gameObject.SetActive(active);
        }
    }
}
