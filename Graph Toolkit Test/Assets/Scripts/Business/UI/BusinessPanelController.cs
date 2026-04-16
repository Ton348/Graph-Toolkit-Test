using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class BusinessPanelController : MonoBehaviour
{
    public GameBootstrap bootstrap;
    public BusinessListView listView;
    public BusinessDetailsView detailsView;
    public TMP_Text statusText;

    private BusinessRuntimeService m_runtimeService;
    private BusinessActionFacade m_actionFacade;
    private BusinessStateSyncService m_stateSync;
    private BusinessDefinitionsRepository m_definitions;
    private GameDataRepository m_gameData;
    private BusinessSimulationService m_simulationService;
    private string m_selectedLotId;

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
            m_runtimeService = bootstrap.BusinessRuntimeService;
            m_actionFacade = bootstrap.BusinessActionFacade;
            m_stateSync = bootstrap.BusinessStateSyncService;
            m_definitions = bootstrap.BusinessDefinitionsRepository;
            m_gameData = bootstrap.GameDataRepository;
            m_simulationService = bootstrap.BusinessSimulationService;
        }
    }

    private void Subscribe()
    {
        if (listView != null)
        {
            listView.selectionChanged += OnBusinessSelected;
        }

        if (detailsView != null)
        {
            detailsView.rentClicked += OnRentClickedAsync;
            detailsView.assignTypeClicked += OnAssignTypeClickedAsync;
            detailsView.installModuleClicked += OnInstallModuleClickedAsync;
            detailsView.assignSupplierClicked += OnAssignSupplierClickedAsync;
            detailsView.hireWorkerClicked += OnHireWorkerClickedAsync;
            detailsView.openClicked += OnOpenClickedAsync;
            detailsView.closeClicked += OnCloseClickedAsync;
            detailsView.setMarkupClicked += OnSetMarkupClickedAsync;
            detailsView.unlockContactClicked += OnUnlockContactClickedAsync;
            detailsView.roleChanged += OnRoleChanged;
        }

        if (bootstrap != null && bootstrap.ProfileSyncService != null)
        {
            bootstrap.ProfileSyncService.synced += OnProfileSynced;
        }

        if (m_stateSync != null)
        {
            m_stateSync.stateChanged += OnStateChanged;
        }

        if (m_simulationService != null)
        {
            m_simulationService.simulationUpdated += OnSimulationUpdated;
        }
    }

    private void Unsubscribe()
    {
        if (listView != null)
        {
            listView.selectionChanged -= OnBusinessSelected;
        }

        if (detailsView != null)
        {
            detailsView.rentClicked -= OnRentClickedAsync;
            detailsView.assignTypeClicked -= OnAssignTypeClickedAsync;
            detailsView.installModuleClicked -= OnInstallModuleClickedAsync;
            detailsView.assignSupplierClicked -= OnAssignSupplierClickedAsync;
            detailsView.hireWorkerClicked -= OnHireWorkerClickedAsync;
            detailsView.openClicked -= OnOpenClickedAsync;
            detailsView.closeClicked -= OnCloseClickedAsync;
            detailsView.setMarkupClicked -= OnSetMarkupClickedAsync;
            detailsView.unlockContactClicked -= OnUnlockContactClickedAsync;
            detailsView.roleChanged -= OnRoleChanged;
        }

        if (bootstrap != null && bootstrap.ProfileSyncService != null)
        {
            bootstrap.ProfileSyncService.synced -= OnProfileSynced;
        }

        if (m_stateSync != null)
        {
            m_stateSync.stateChanged -= OnStateChanged;
        }

        if (m_simulationService != null)
        {
            m_simulationService.simulationUpdated -= OnSimulationUpdated;
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
        if (m_runtimeService == null || listView == null)
        {
            return;
        }

        listView.SetBusinesses(m_runtimeService.GetBusinesses(), b => ResolveLotDisplayName(b != null ? b.lotId : null));
    }

    private void OnBusinessSelected(BusinessInstanceSnapshot business)
    {
        if (detailsView == null)
        {
            return;
        }

        if (business != null && !string.IsNullOrWhiteSpace(business.lotId))
        {
            m_selectedLotId = business.lotId;
        }

        var required = new List<string>();
        var missing = new List<string>();

        if (business != null && m_definitions != null)
        {
            required.AddRange(m_definitions.GetRequiredModules(business.businessTypeId));
            if (m_runtimeService != null)
            {
                missing.AddRange(m_runtimeService.GetMissingRequiredModules(business));
            }
        }

        var knownContactIds = m_stateSync != null ? m_stateSync.GetKnownContacts() : new List<string>();
        string selectedLot = business != null ? business.lotId : m_selectedLotId;
        PopulateDropdowns(business, selectedLot);
        var simulation = m_simulationService != null && !string.IsNullOrWhiteSpace(m_selectedLotId)
            ? m_simulationService.GetStateByLotId(m_selectedLotId)
            : null;
        detailsView.SetBusiness(
            business,
            simulation,
            required.Select(ResolveModuleDisplayName),
            missing.Select(ResolveModuleDisplayName),
            business != null ? ResolveLotDisplayName(business.lotId) : null,
            business != null ? ResolveBusinessTypeDisplayName(business.businessTypeId) : null,
            knownContactIds.Select(ResolveContactDisplayName),
            business != null ? ResolveContactDisplayName(business.selectedSupplierId) : null,
            business != null ? ResolveContactDisplayName(business.hiredCashierContactId) : null,
            business != null ? ResolveContactDisplayName(business.hiredMerchContactId) : null);
    }

    private void RefreshSelected()
    {
        if (detailsView == null || m_runtimeService == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(m_selectedLotId))
        {
            return;
        }

        var business = m_runtimeService.GetBusinessView(m_selectedLotId);
        OnBusinessSelected(business);
    }

    public void OpenForLot(string lotId)
    {
        m_selectedLotId = lotId;
        RefreshSelected();
    }

    private async void OnRentClickedAsync()
    {
        if (m_actionFacade == null || detailsView == null) return;
        string lotId = detailsView.GetLotId();
        BusinessDebugLog.Log($"[BusinessUI] Rent lotId='{lotId}'");
        await RunActionAsync(m_actionFacade.RentBusiness(lotId));
    }

    private async void OnAssignTypeClickedAsync()
    {
        if (m_actionFacade == null || detailsView == null) return;
        string lotId = detailsView.GetLotId();
        string businessTypeId = detailsView.GetBusinessTypeId();
        BusinessDebugLog.Log($"[BusinessUI] AssignType lotId='{lotId}' businessTypeId='{businessTypeId}'");
        await RunActionAsync(m_actionFacade.AssignBusinessType(lotId, businessTypeId));
    }

    private async void OnInstallModuleClickedAsync()
    {
        if (m_actionFacade == null || detailsView == null) return;
        string lotId = detailsView.GetLotId();
        string moduleId = detailsView.GetModuleId();
        BusinessDebugLog.Log($"[BusinessUI] InstallModule lotId='{lotId}' moduleId='{moduleId}'");
        await RunActionAsync(m_actionFacade.InstallModule(lotId, moduleId));
    }

    private async void OnAssignSupplierClickedAsync()
    {
        if (m_actionFacade == null || detailsView == null) return;
        string lotId = detailsView.GetLotId();
        string supplierId = detailsView.GetSupplierId();
        BusinessDebugLog.Log($"[BusinessUI] AssignSupplier lotId='{lotId}' supplierId='{supplierId}'");
        await RunActionAsync(m_actionFacade.AssignSupplier(lotId, supplierId));
    }

    private async void OnHireWorkerClickedAsync()
    {
        if (m_actionFacade == null || detailsView == null) return;
        string lotId = detailsView.GetLotId();
        string roleId = detailsView.GetRoleId();
        string contactId = detailsView.GetContactId();
        BusinessDebugLog.Log($"[BusinessUI] HireWorker lotId='{lotId}' roleId='{roleId}' contactId='{contactId}'");
        await RunActionAsync(m_actionFacade.HireWorker(lotId, roleId, contactId));
    }

    private async void OnOpenClickedAsync()
    {
        if (m_actionFacade == null || detailsView == null) return;
        string lotId = detailsView.GetLotId();
        BusinessDebugLog.Log($"[BusinessUI] OpenBusiness lotId='{lotId}'");
        await RunActionAsync(m_actionFacade.OpenBusiness(lotId));
    }

    private async void OnCloseClickedAsync()
    {
        if (m_actionFacade == null || detailsView == null) return;
        string lotId = detailsView.GetLotId();
        BusinessDebugLog.Log($"[BusinessUI] CloseBusiness lotId='{lotId}'");
        await RunActionAsync(m_actionFacade.CloseBusiness(lotId));
    }

    private async void OnSetMarkupClickedAsync()
    {
        if (m_actionFacade == null || detailsView == null) return;
        string lotId = detailsView.GetLotId();
        int markup = detailsView.GetMarkupPercent();
        BusinessDebugLog.Log($"[BusinessUI] SetMarkup lotId='{lotId}' markupPercent={markup}");
        await RunActionAsync(m_actionFacade.SetMarkup(lotId, markup));
    }

    private async void OnUnlockContactClickedAsync()
    {
        if (m_actionFacade == null || detailsView == null) return;
        string contactId = detailsView.GetUnlockContactId();
        BusinessDebugLog.Log($"[BusinessUI] UnlockContact contactId='{contactId}'");
        await RunActionAsync(m_actionFacade.UnlockContact(contactId));
    }

    private void OnRoleChanged()
    {
        if (detailsView == null)
        {
            return;
        }

        string roleId = detailsView.GetRoleId();
        var business = m_runtimeService != null && !string.IsNullOrWhiteSpace(m_selectedLotId)
            ? m_runtimeService.GetBusinessView(m_selectedLotId)
            : null;
        PopulateWorkerDropdown(roleId, business);
    }

    private async Task RunActionAsync(Task<ServerActionResult> actionTask)
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

    private void PopulateDropdowns(BusinessInstanceSnapshot business, string lotId)
    {
        if (detailsView == null)
        {
            return;
        }

        var selectedRole = detailsView.GetRoleId();
        var selectedLot = !string.IsNullOrWhiteSpace(lotId) ? lotId : detailsView.GetLotId();
        var selectedType = business != null ? business.businessTypeId : detailsView.GetBusinessTypeId();
        var selectedSupplier = business != null ? business.selectedSupplierId : detailsView.GetSupplierId();
        var selectedModule = detailsView.GetModuleId();
        var selectedUnlock = detailsView.GetUnlockContactId();

        detailsView.SetLotOptions(BuildLotOptions(), selectedLot);
        detailsView.SetBusinessTypeOptions(BuildBusinessTypeOptions(selectedLot), selectedType);
        detailsView.SetModuleOptions(BuildModuleOptions(business), selectedModule);
        detailsView.SetSupplierOptions(BuildSupplierOptions(business), selectedSupplier);
        detailsView.SetRoleOptions(BuildRoleOptions(), selectedRole);
        PopulateWorkerDropdown(detailsView.GetRoleId(), business);
        detailsView.SetUnlockContactOptions(BuildUnlockContactOptions(), selectedUnlock);
    }

    private void PopulateWorkerDropdown(string roleId, BusinessInstanceSnapshot business)
    {
        if (detailsView == null)
        {
            return;
        }

        string selected = null;
        if (business != null)
        {
            if (roleId == "cashier")
            {
                selected = business.hiredCashierContactId;
            }
            else if (roleId == "merchandiser")
            {
                selected = business.hiredMerchContactId;
            }
        }

        detailsView.SetWorkerContactOptions(BuildWorkerContactOptions(roleId), selected);
    }

    private IEnumerable<BusinessDetailsView.IdOption> BuildLotOptions()
    {
        var options = new List<BusinessDetailsView.IdOption>();
        if (m_gameData == null)
        {
            return options;
        }

        foreach (var lot in m_gameData.GetAllLots())
        {
            if (lot == null || string.IsNullOrWhiteSpace(lot.id))
            {
                continue;
            }

            options.Add(new BusinessDetailsView.IdOption
            {
                id = lot.id,
                displayName = !string.IsNullOrWhiteSpace(lot.displayName) ? lot.displayName : lot.id
            });
        }

        return options;
    }

    private IEnumerable<BusinessDetailsView.IdOption> BuildBusinessTypeOptions(string lotId)
    {
        var options = new List<BusinessDetailsView.IdOption>();
        if (m_definitions == null)
        {
            return options;
        }

        HashSet<string> allowed = null;
        if (!string.IsNullOrWhiteSpace(lotId) && m_gameData != null)
        {
            var lot = m_gameData.GetLotById(lotId);
            if (lot != null && lot.allowedBusinessTypes != null && lot.allowedBusinessTypes.Count > 0)
            {
                allowed = new HashSet<string>(lot.allowedBusinessTypes);
            }
        }

        foreach (var type in m_definitions.GetAllBusinessTypes())
        {
            if (type == null || string.IsNullOrWhiteSpace(type.id))
            {
                continue;
            }

            if (allowed != null && !allowed.Contains(type.id))
            {
                continue;
            }

            options.Add(new BusinessDetailsView.IdOption
            {
                id = type.id,
                displayName = !string.IsNullOrWhiteSpace(type.displayName) ? type.displayName : type.id
            });
        }

        return options;
    }

    private IEnumerable<BusinessDetailsView.IdOption> BuildModuleOptions(BusinessInstanceSnapshot business)
    {
        var options = new List<BusinessDetailsView.IdOption>();
        if (m_definitions == null)
        {
            return options;
        }

        var installed = business != null && business.installedModules != null
            ? new HashSet<string>(business.installedModules)
            : new HashSet<string>();

        foreach (var module in m_definitions.GetAllModules())
        {
            if (module == null || string.IsNullOrWhiteSpace(module.id))
            {
                continue;
            }

            if (installed.Contains(module.id))
            {
                continue;
            }

            options.Add(new BusinessDetailsView.IdOption
            {
                id = module.id,
                displayName = !string.IsNullOrWhiteSpace(module.displayName) ? module.displayName : module.id
            });
        }

        return options;
    }

    private IEnumerable<BusinessDetailsView.IdOption> BuildSupplierOptions(BusinessInstanceSnapshot business)
    {
        var options = new List<BusinessDetailsView.IdOption>();
        if (m_runtimeService == null || m_definitions == null || m_stateSync == null)
        {
            return options;
        }

        IEnumerable<SupplierDefinitionData> suppliers;
        if (business != null)
        {
            suppliers = m_runtimeService.GetAvailableSuppliers(business);
        }
        else
        {
            var known = new HashSet<string>(m_stateSync.GetKnownContacts());
            var filtered = new List<SupplierDefinitionData>();
            foreach (var supplier in m_definitions.GetAllSuppliers())
            {
                if (supplier != null && !string.IsNullOrWhiteSpace(supplier.id) && known.Contains(supplier.id))
                {
                    filtered.Add(supplier);
                }
            }
            suppliers = filtered;
        }

        foreach (var supplier in suppliers)
        {
            if (supplier == null || string.IsNullOrWhiteSpace(supplier.id))
            {
                continue;
            }

            options.Add(new BusinessDetailsView.IdOption
            {
                id = supplier.id,
                displayName = !string.IsNullOrWhiteSpace(supplier.displayName) ? supplier.displayName : supplier.id
            });
        }

        return options;
    }

    private IEnumerable<BusinessDetailsView.IdOption> BuildRoleOptions()
    {
        var options = new List<BusinessDetailsView.IdOption>();
        if (m_definitions == null)
        {
            return options;
        }

        foreach (var role in m_definitions.GetAllStaffRoles())
        {
            if (role == null || string.IsNullOrWhiteSpace(role.id))
            {
                continue;
            }

            options.Add(new BusinessDetailsView.IdOption
            {
                id = role.id,
                displayName = !string.IsNullOrWhiteSpace(role.displayName) ? role.displayName : role.id
            });
        }

        return options;
    }

    private IEnumerable<BusinessDetailsView.IdOption> BuildWorkerContactOptions(string roleId)
    {
        var options = new List<BusinessDetailsView.IdOption>();
        if (string.IsNullOrWhiteSpace(roleId) || m_definitions == null || m_stateSync == null)
        {
            return options;
        }

        var known = new HashSet<string>(m_stateSync.GetKnownContacts());
        foreach (var contact in m_definitions.GetStaffContactsByRole(roleId))
        {
            if (contact == null || string.IsNullOrWhiteSpace(contact.id))
            {
                continue;
            }

            if (!known.Contains(contact.id))
            {
                continue;
            }

            options.Add(new BusinessDetailsView.IdOption
            {
                id = contact.id,
                displayName = !string.IsNullOrWhiteSpace(contact.displayName) ? contact.displayName : contact.id
            });
        }

        return options;
    }

    private IEnumerable<BusinessDetailsView.IdOption> BuildUnlockContactOptions()
    {
        var options = new List<BusinessDetailsView.IdOption>();
        if (m_definitions == null || m_stateSync == null)
        {
            return options;
        }

        var known = new HashSet<string>(m_stateSync.GetKnownContacts());
        var used = new HashSet<string>();

        foreach (var supplier in m_definitions.GetAllSuppliers())
        {
            if (supplier == null || string.IsNullOrWhiteSpace(supplier.id) || known.Contains(supplier.id) || !used.Add(supplier.id))
            {
                continue;
            }

            options.Add(new BusinessDetailsView.IdOption
            {
                id = supplier.id,
                displayName = !string.IsNullOrWhiteSpace(supplier.displayName) ? supplier.displayName : supplier.id
            });
        }

        foreach (var contact in m_definitions.GetAllStaffContacts())
        {
            if (contact == null || string.IsNullOrWhiteSpace(contact.id) || known.Contains(contact.id) || !used.Add(contact.id))
            {
                continue;
            }

            var roleDisplay = ResolveRoleDisplayName(contact.roleId);
            var display = !string.IsNullOrWhiteSpace(contact.displayName) ? contact.displayName : contact.id;
            options.Add(new BusinessDetailsView.IdOption
            {
                id = contact.id,
                displayName = string.IsNullOrWhiteSpace(roleDisplay) ? display : $"{display} ({roleDisplay})"
            });
        }

        return options;
    }

    private string ResolveModuleDisplayName(string moduleId)
    {
        if (string.IsNullOrWhiteSpace(moduleId))
        {
            return "-";
        }

        var module = m_definitions != null ? m_definitions.GetModule(moduleId) : null;
        return module != null && !string.IsNullOrWhiteSpace(module.displayName) ? module.displayName : moduleId;
    }

    private string ResolveRoleDisplayName(string roleId)
    {
        if (string.IsNullOrWhiteSpace(roleId))
        {
            return null;
        }

        var role = m_definitions != null ? m_definitions.GetStaffRole(roleId) : null;
        return role != null && !string.IsNullOrWhiteSpace(role.displayName) ? role.displayName : roleId;
    }

    private string ResolveLotDisplayName(string lotId)
    {
        if (string.IsNullOrWhiteSpace(lotId))
        {
            return "-";
        }

        var lot = m_gameData != null ? m_gameData.GetLotById(lotId) : null;
        return lot != null && !string.IsNullOrWhiteSpace(lot.displayName) ? lot.displayName : lotId;
    }

    private string ResolveBusinessTypeDisplayName(string businessTypeId)
    {
        if (string.IsNullOrWhiteSpace(businessTypeId))
        {
            return "-";
        }

        var type = m_definitions != null ? m_definitions.GetBusinessType(businessTypeId) : null;
        return type != null && !string.IsNullOrWhiteSpace(type.displayName) ? type.displayName : businessTypeId;
    }

    private string ResolveContactDisplayName(string contactId)
    {
        if (string.IsNullOrWhiteSpace(contactId))
        {
            return "-";
        }

        var supplier = m_definitions != null ? m_definitions.GetSupplier(contactId) : null;
        if (supplier != null && !string.IsNullOrWhiteSpace(supplier.displayName))
        {
            return supplier.displayName;
        }

        var contact = m_definitions != null ? m_definitions.GetStaffContact(contactId) : null;
        if (contact != null && !string.IsNullOrWhiteSpace(contact.displayName))
        {
            return contact.displayName;
        }

        return contactId;
    }
}
