using System.Collections.Generic;
using System.Linq;
using Prototype.Business.Bootstrap;
using Prototype.Business.Data;
using Prototype.Business.Runtime;
using Prototype.Business.Services;
using Prototype.Business.Simulation;
using Sample.Runtime.GameData;
using TMPro;
using UnityEngine;

namespace Prototype.Business.UI
{
	public sealed class BusinessPanelController : MonoBehaviour
	{
		public GameBootstrap bootstrap;
		public BusinessListView listView;
		public BusinessDetailsView detailsView;
		public TMP_Text statusText;

		private BusinessManagementController m_managementController;
		private BusinessActionFacade m_actionFacade;
		private BusinessDefinitionsRepository m_definitions;
		private GameDataRepository m_gameData;
		private BusinessRuntimeService m_runtimeService;
		private BusinessSimulationService m_simulationService;
		private BusinessStateSyncService m_stateSync;
		private string m_selectedLotId;
		private ProfileSyncService m_profileSync;

		private void OnEnable()
		{
			EnsureDependencies();
			Subscribe();
			Refresh();
			LogPanelSnapshot("OnEnable");
		}

		private void OnDisable()
		{
			Unsubscribe();
		}

		private void EnsureDependencies()
		{
			ProfileSyncService previousProfileSync = m_profileSync;
			BusinessStateSyncService previousStateSync = m_stateSync;
			BusinessSimulationService previousSimulation = m_simulationService;

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
				m_profileSync = bootstrap.ProfileSyncService;
			}
			else
			{
				m_profileSync = null;
			}

			if (m_managementController == null)
			{
				m_managementController = new BusinessManagementController();
			}

			m_managementController.Initialize(
				detailsView,
				m_actionFacade,
				m_runtimeService,
				m_definitions,
				m_gameData,
				SetStatus,
				OnBusinessChanged);

			if (previousProfileSync != m_profileSync)
			{
				if (previousProfileSync != null)
				{
					previousProfileSync.synced -= OnProfileSynced;
				}

				if (m_profileSync != null)
				{
					m_profileSync.synced -= OnProfileSynced;
					m_profileSync.synced += OnProfileSynced;
				}
			}

			if (previousStateSync != m_stateSync)
			{
				if (previousStateSync != null)
				{
					previousStateSync.stateChanged -= OnStateChanged;
				}

				if (m_stateSync != null)
				{
					m_stateSync.stateChanged -= OnStateChanged;
					m_stateSync.stateChanged += OnStateChanged;
				}
			}

			if (previousSimulation != m_simulationService)
			{
				if (previousSimulation != null)
				{
					previousSimulation.simulationUpdated -= OnSimulationUpdated;
				}

				if (m_simulationService != null)
				{
					m_simulationService.simulationUpdated -= OnSimulationUpdated;
					m_simulationService.simulationUpdated += OnSimulationUpdated;
				}
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
				detailsView.closeClicked += OnWindowCloseClicked;
			}

			if (m_profileSync != null)
			{
				m_profileSync.synced += OnProfileSynced;
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
				detailsView.closeClicked -= OnWindowCloseClicked;
			}

			if (m_profileSync != null)
			{
				m_profileSync.synced -= OnProfileSynced;
			}

			if (m_stateSync != null)
			{
				m_stateSync.stateChanged -= OnStateChanged;
			}

			if (m_simulationService != null)
			{
				m_simulationService.simulationUpdated -= OnSimulationUpdated;
			}

			m_managementController?.Dispose();
		}

		private void OnProfileSynced(ProfileSnapshot snapshot)
		{
			EnsureDependencies();
			Refresh();
			LogPanelSnapshot("OnProfileSynced");
		}

		private void OnStateChanged()
		{
			EnsureDependencies();
			Refresh();
			LogPanelSnapshot("OnStateChanged");
		}

		private void OnSimulationUpdated()
		{
			RefreshSelected();
		}

		private void Refresh()
		{
			if (m_runtimeService == null)
			{
				EnsureDependencies();
			}

			if (m_runtimeService == null)
			{
				return;
			}

			List<BusinessInstanceSnapshot> businesses = m_runtimeService.GetBusinesses().ToList();
			if (listView != null)
			{
				listView.SetBusinesses(businesses,
					business => ResolveLotDisplayName(business != null ? business.lotId : null));
			}

			if (string.IsNullOrWhiteSpace(m_selectedLotId) && businesses.Count > 0)
			{
				m_selectedLotId = businesses[0].lotId;
			}

			RefreshSelected();
		}

		private void LogPanelSnapshot(string source)
		{
			List<BusinessInstanceSnapshot> businesses = m_runtimeService != null
				? m_runtimeService.GetBusinesses().Where(b => b != null).ToList()
				: new List<BusinessInstanceSnapshot>();

			List<string> businessLots = businesses
				.Select(b => $"{b.lotId}|type={b.businessTypeId}|open={b.isOpen}")
				.ToList();

			List<string> knownContacts = m_stateSync != null
				? m_stateSync.GetKnownContacts().Where(id => !string.IsNullOrWhiteSpace(id)).ToList()
				: new List<string>();

			List<string> cashierOptions = BuildWorkerContactOptions()
				.Where(o => o != null && !string.IsNullOrWhiteSpace(o.id))
				.Select(o => $"{o.id}:{o.displayName}")
				.ToList();

			List<string> merchOptions = BuildWorkerContactOptions()
				.Where(o => o != null && !string.IsNullOrWhiteSpace(o.id))
				.Select(o => $"{o.id}:{o.displayName}")
				.ToList();

			List<string> logistOptions = BuildWorkerContactOptions()
				.Where(o => o != null && !string.IsNullOrWhiteSpace(o.id))
				.Select(o => $"{o.id}:{o.displayName}")
				.ToList();

			Debug.Log(
				$"[BusinessPanelDebug] {source} " +
				$"runtime={(m_runtimeService != null)} stateSync={(m_stateSync != null)} defs={(m_definitions != null)} " +
				$"businesses={businesses.Count} [{string.Join(", ", businessLots)}] " +
				$"knownContacts={knownContacts.Count} [{string.Join(", ", knownContacts)}] " +
				$"cashiers={cashierOptions.Count} [{string.Join(", ", cashierOptions)}] " +
				$"merchandisers={merchOptions.Count} [{string.Join(", ", merchOptions)}] " +
				$"logists={logistOptions.Count} [{string.Join(", ", logistOptions)}]");
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

			var requiredModules = new List<string>();
			var missingModules = new List<string>();
			if (business != null && m_definitions != null)
			{
				requiredModules.AddRange(m_definitions.GetRequiredModules(business.businessTypeId));
				if (m_runtimeService != null)
				{
					missingModules.AddRange(m_runtimeService.GetMissingRequiredModules(business));
				}
			}

			IReadOnlyCollection<string> knownContactIds =
				m_stateSync != null ? m_stateSync.GetKnownContacts() : new List<string>();

			PopulateDropdowns(business, business != null ? business.lotId : m_selectedLotId);

			BusinessRuntimeSimulationState simulation =
				m_simulationService != null && !string.IsNullOrWhiteSpace(m_selectedLotId)
					? m_simulationService.GetStateByLotId(m_selectedLotId)
					: null;

			detailsView.SetBusiness(
				business,
				simulation,
				requiredModules.Select(ResolveModuleDisplayName),
				missingModules.Select(ResolveModuleDisplayName),
				business != null ? ResolveLotDisplayName(business.lotId) : null,
				business != null ? ResolveBusinessTypeDisplayName(business.businessTypeId) : null,
				knownContactIds.Select(ResolveContactDisplayName),
				business != null ? ResolveContactDisplayName(business.selectedSupplierId) : null,
				business != null ? ResolveContactDisplayName(business.hiredCashierContactId) : null,
				business != null ? ResolveContactDisplayName(business.hiredMerchContactId) : null);
		}

		private void RefreshSelected()
		{
			if (detailsView == null || m_runtimeService == null || string.IsNullOrWhiteSpace(m_selectedLotId))
			{
				return;
			}

			BusinessInstanceSnapshot business = m_runtimeService.GetBusinessView(m_selectedLotId);
			OnBusinessSelected(business);
		}

		public void OpenForLot(string lotId)
		{
			m_selectedLotId = lotId;
			RefreshSelected();
		}

		private void OnWindowCloseClicked()
		{
			gameObject.SetActive(false);
		}

		private void OnBusinessChanged(string lotId)
		{
			if (string.IsNullOrWhiteSpace(lotId))
			{
				return;
			}

			m_selectedLotId = lotId;
			RefreshSelected();
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

			string selectedLot = !string.IsNullOrWhiteSpace(lotId) ? lotId : detailsView.GetSelectedBusinessId();
			detailsView.SetBusinessOptions(BuildBusinessOptions(), selectedLot);

			List<BusinessDetailsView.IdOption> moduleOptions = BuildModuleOptions(business).ToList();
			detailsView.SetStorageOptions(moduleOptions, detailsView.GetPendingStorageId());
			detailsView.SetCashDeskOptions(moduleOptions, detailsView.GetPendingCashDeskId());
			detailsView.SetShelfOptions(moduleOptions, detailsView.GetPendingShelfId());

			IEnumerable<BusinessDetailsView.IdOption> contacts = BuildWorkerContactOptions();
			detailsView.SetSupplierOptions(contacts, detailsView.GetPendingSupplierId());
			detailsView.SetCashierOptions(contacts, detailsView.GetPendingCashierId());
			detailsView.SetMerchandiserOptions(contacts, detailsView.GetPendingMerchandiserId());
		}

		private IEnumerable<BusinessDetailsView.IdOption> BuildBusinessOptions()
		{
			var options = new List<BusinessDetailsView.IdOption>();
			if (m_runtimeService == null)
			{
				return options;
			}

			foreach (BusinessInstanceSnapshot business in m_runtimeService.GetBusinesses())
			{
				if (business == null || string.IsNullOrWhiteSpace(business.lotId))
				{
					continue;
				}

				options.Add(new BusinessDetailsView.IdOption
				{
					id = business.lotId,
					displayName = ResolveLotDisplayName(business.lotId)
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

			HashSet<string> installed = business != null && business.installedModules != null
				? new HashSet<string>(business.installedModules)
				: new HashSet<string>();

			foreach (BusinessModuleDefinitionData module in m_definitions.GetAllModules())
			{
				if (module == null || string.IsNullOrWhiteSpace(module.id) || installed.Contains(module.id))
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
				suppliers = m_definitions.GetAllSuppliers().Where(supplier =>
					supplier != null && !string.IsNullOrWhiteSpace(supplier.id) && known.Contains(supplier.id));
			}

			foreach (SupplierDefinitionData supplier in suppliers)
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

		private IEnumerable<BusinessDetailsView.IdOption> BuildWorkerContactOptions()
		{
			var options = new List<BusinessDetailsView.IdOption>();
			if (m_definitions == null || m_stateSync == null)
			{
				return options;
			}

			HashSet<string> known = new HashSet<string>(m_stateSync.GetKnownContacts());
			foreach (StaffContactDefinitionData contact in m_definitions.GetAllStaffContacts())
			{
				if (contact == null || string.IsNullOrWhiteSpace(contact.id) || !known.Contains(contact.id))
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

		private string ResolveModuleDisplayName(string moduleId)
		{
			if (string.IsNullOrWhiteSpace(moduleId))
			{
				return "-";
			}

			BusinessModuleDefinitionData module = m_definitions != null ? m_definitions.GetModule(moduleId) : null;
			return module != null && !string.IsNullOrWhiteSpace(module.displayName) ? module.displayName : moduleId;
		}

		private string ResolveLotDisplayName(string lotId)
		{
			if (string.IsNullOrWhiteSpace(lotId))
			{
				return "-";
			}

			LotDefinitionData lot = m_gameData != null ? m_gameData.GetLotById(lotId) : null;
			return lot != null && !string.IsNullOrWhiteSpace(lot.displayName) ? lot.displayName : lotId;
		}

		private string ResolveBusinessTypeDisplayName(string businessTypeId)
		{
			if (string.IsNullOrWhiteSpace(businessTypeId))
			{
				return "-";
			}

			BusinessTypeDefinitionData type = m_definitions != null ? m_definitions.GetBusinessType(businessTypeId) : null;
			return type != null && !string.IsNullOrWhiteSpace(type.displayName) ? type.displayName : businessTypeId;
		}

		private string ResolveContactDisplayName(string contactId)
		{
			if (string.IsNullOrWhiteSpace(contactId))
			{
				return "-";
			}

			SupplierDefinitionData supplier = m_definitions != null ? m_definitions.GetSupplier(contactId) : null;
			if (supplier != null && !string.IsNullOrWhiteSpace(supplier.displayName))
			{
				return supplier.displayName;
			}

			StaffContactDefinitionData contact = m_definitions != null ? m_definitions.GetStaffContact(contactId) : null;
			if (contact != null && !string.IsNullOrWhiteSpace(contact.displayName))
			{
				return contact.displayName;
			}

			return contactId;
		}
	}
}
