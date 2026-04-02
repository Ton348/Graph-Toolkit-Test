using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class BusinessPanelController : MonoBehaviour
{
    public GameBootstrap bootstrap;
    public BusinessListView listView;
    public BusinessDetailsView detailsView;
    public TMP_Text statusText;

    private BusinessRuntimeService runtimeService;
    private BusinessActionFacade actionFacade;
    private BusinessStateSyncService stateSync;
    private BusinessDefinitionsRepository definitions;
    private BusinessSimulationService simulationService;
    private string selectedLotId;

    private void OnEnable()
    {
        EnsureDependencies();
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void EnsureDependencies()
    {
        if (bootstrap == null)
        {
            bootstrap = FindObjectOfType<GameBootstrap>();
        }

        if (bootstrap != null)
        {
            runtimeService = bootstrap.BusinessRuntimeService;
            actionFacade = bootstrap.BusinessActionFacade;
            stateSync = bootstrap.BusinessStateSyncService;
            definitions = bootstrap.BusinessDefinitionsRepository;
            simulationService = bootstrap.BusinessSimulationService;
        }
    }

    private void Subscribe()
    {
        if (listView != null)
        {
            listView.SelectionChanged += OnBusinessSelected;
        }

        if (detailsView != null)
        {
            detailsView.RentClicked += OnRentClicked;
            detailsView.AssignTypeClicked += OnAssignTypeClicked;
            detailsView.InstallModuleClicked += OnInstallModuleClicked;
            detailsView.AssignSupplierClicked += OnAssignSupplierClicked;
            detailsView.HireWorkerClicked += OnHireWorkerClicked;
            detailsView.OpenClicked += OnOpenClicked;
            detailsView.CloseClicked += OnCloseClicked;
            detailsView.SetMarkupClicked += OnSetMarkupClicked;
            detailsView.UnlockContactClicked += OnUnlockContactClicked;
        }

        if (bootstrap != null && bootstrap.ProfileSyncService != null)
        {
            bootstrap.ProfileSyncService.Synced += OnProfileSynced;
        }

        if (stateSync != null)
        {
            stateSync.StateChanged += OnStateChanged;
        }

        if (simulationService != null)
        {
            simulationService.SimulationUpdated += OnSimulationUpdated;
        }
    }

    private void Unsubscribe()
    {
        if (listView != null)
        {
            listView.SelectionChanged -= OnBusinessSelected;
        }

        if (detailsView != null)
        {
            detailsView.RentClicked -= OnRentClicked;
            detailsView.AssignTypeClicked -= OnAssignTypeClicked;
            detailsView.InstallModuleClicked -= OnInstallModuleClicked;
            detailsView.AssignSupplierClicked -= OnAssignSupplierClicked;
            detailsView.HireWorkerClicked -= OnHireWorkerClicked;
            detailsView.OpenClicked -= OnOpenClicked;
            detailsView.CloseClicked -= OnCloseClicked;
            detailsView.SetMarkupClicked -= OnSetMarkupClicked;
            detailsView.UnlockContactClicked -= OnUnlockContactClicked;
        }

        if (bootstrap != null && bootstrap.ProfileSyncService != null)
        {
            bootstrap.ProfileSyncService.Synced -= OnProfileSynced;
        }

        if (stateSync != null)
        {
            stateSync.StateChanged -= OnStateChanged;
        }

        if (simulationService != null)
        {
            simulationService.SimulationUpdated -= OnSimulationUpdated;
        }
    }

    private void OnProfileSynced(ProfileSnapshot snapshot)
    {
        Refresh();
    }

    private void OnStateChanged()
    {
        Refresh();
    }

    private void OnSimulationUpdated()
    {
        RefreshSelected();
    }

    private void Refresh()
    {
        if (runtimeService == null || listView == null)
        {
            return;
        }

        listView.SetBusinesses(runtimeService.GetBusinesses());
    }

    private void OnBusinessSelected(BusinessInstanceSnapshot business)
    {
        if (detailsView == null)
        {
            return;
        }

        selectedLotId = business != null ? business.lotId : null;

        var required = new List<string>();
        var missing = new List<string>();

        if (business != null && definitions != null)
        {
            required.AddRange(definitions.GetRequiredModules(business.businessTypeId));
            if (runtimeService != null)
            {
                missing.AddRange(runtimeService.GetMissingRequiredModules(business));
            }
        }

        IEnumerable<string> contacts = stateSync != null ? stateSync.GetKnownContacts() : new List<string>();
        var simulation = simulationService != null && !string.IsNullOrWhiteSpace(selectedLotId)
            ? simulationService.GetStateByLotId(selectedLotId)
            : null;
        detailsView.SetBusiness(business, simulation, required, missing, contacts);
    }

    private void RefreshSelected()
    {
        if (detailsView == null || runtimeService == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(selectedLotId))
        {
            return;
        }

        var business = runtimeService.GetBusinessView(selectedLotId);
        OnBusinessSelected(business);
    }

    public void OpenForLot(string lotId)
    {
        selectedLotId = lotId;
        RefreshSelected();
    }

    private async void OnRentClicked()
    {
        if (actionFacade == null || detailsView == null) return;
        string lotId = detailsView.GetLotId();
        BusinessDebugLog.Log($"[BusinessUI] Rent lotId='{lotId}'");
        await RunAction(actionFacade.RentBusiness(lotId));
    }

    private async void OnAssignTypeClicked()
    {
        if (actionFacade == null || detailsView == null) return;
        string lotId = detailsView.GetLotId();
        string businessTypeId = detailsView.GetBusinessTypeId();
        BusinessDebugLog.Log($"[BusinessUI] AssignType lotId='{lotId}' businessTypeId='{businessTypeId}'");
        await RunAction(actionFacade.AssignBusinessType(lotId, businessTypeId));
    }

    private async void OnInstallModuleClicked()
    {
        if (actionFacade == null || detailsView == null) return;
        string lotId = detailsView.GetLotId();
        string moduleId = detailsView.GetModuleId();
        BusinessDebugLog.Log($"[BusinessUI] InstallModule lotId='{lotId}' moduleId='{moduleId}'");
        await RunAction(actionFacade.InstallModule(lotId, moduleId));
    }

    private async void OnAssignSupplierClicked()
    {
        if (actionFacade == null || detailsView == null) return;
        string lotId = detailsView.GetLotId();
        string supplierId = detailsView.GetSupplierId();
        BusinessDebugLog.Log($"[BusinessUI] AssignSupplier lotId='{lotId}' supplierId='{supplierId}'");
        await RunAction(actionFacade.AssignSupplier(lotId, supplierId));
    }

    private async void OnHireWorkerClicked()
    {
        if (actionFacade == null || detailsView == null) return;
        string lotId = detailsView.GetLotId();
        string roleId = detailsView.GetRoleId();
        string contactId = detailsView.GetContactId();
        BusinessDebugLog.Log($"[BusinessUI] HireWorker lotId='{lotId}' roleId='{roleId}' contactId='{contactId}'");
        await RunAction(actionFacade.HireWorker(lotId, roleId, contactId));
    }

    private async void OnOpenClicked()
    {
        if (actionFacade == null || detailsView == null) return;
        string lotId = detailsView.GetLotId();
        BusinessDebugLog.Log($"[BusinessUI] OpenBusiness lotId='{lotId}'");
        await RunAction(actionFacade.OpenBusiness(lotId));
    }

    private async void OnCloseClicked()
    {
        if (actionFacade == null || detailsView == null) return;
        string lotId = detailsView.GetLotId();
        BusinessDebugLog.Log($"[BusinessUI] CloseBusiness lotId='{lotId}'");
        await RunAction(actionFacade.CloseBusiness(lotId));
    }

    private async void OnSetMarkupClicked()
    {
        if (actionFacade == null || detailsView == null) return;
        string lotId = detailsView.GetLotId();
        int markup = detailsView.GetMarkupPercent();
        BusinessDebugLog.Log($"[BusinessUI] SetMarkup lotId='{lotId}' markupPercent={markup}");
        await RunAction(actionFacade.SetMarkup(lotId, markup));
    }

    private async void OnUnlockContactClicked()
    {
        if (actionFacade == null || detailsView == null) return;
        string contactId = detailsView.GetContactId();
        BusinessDebugLog.Log($"[BusinessUI] UnlockContact contactId='{contactId}'");
        await RunAction(actionFacade.UnlockContact(contactId));
    }

    private async Task RunAction(Task<ServerActionResult> actionTask)
    {
        if (actionTask == null)
        {
            SetStatus("No action.");
            return;
        }

        var result = await actionTask;
        if (result == null)
        {
            SetStatus("No response.");
            return;
        }

        SetStatus(result.Success ? $"Success: {result.Message}" : $"Fail: {result.ErrorCode}");
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}
