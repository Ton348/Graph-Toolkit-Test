using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
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

		[Header("Day Simulation")]
		[Min(1f)]
		public float secondsPerBusinessDay = 60f;
		public bool autoSimulateServerBusinessDay;

		[Header("UI Refresh")]
		[Min(0.1f)]
		public float uiRefreshIntervalSeconds = 1f;

		private BusinessManagementController m_managementController;
		private BusinessActionFacade m_actionFacade;
		private BusinessDefinitionsRepository m_definitions;
		private GameDataRepository m_gameData;
		private BusinessRuntimeService m_runtimeService;
		private BusinessSimulationService m_simulationService;
		private BusinessStateSyncService m_stateSync;
		private PlayerStateSync m_playerStateSync;
		private BusinessCalculationService m_businessCalculation;
		private string m_selectedLotId;
		private ProfileSyncService m_profileSync;
		private float m_businessDayTimer;
		private float m_uiRefreshTimer;
		private bool m_daySimulationInProgress;

		private void OnEnable()
		{
			EnsureDependencies();
			Subscribe();
			m_businessDayTimer = 0f;
			m_uiRefreshTimer = 0f;
			m_daySimulationInProgress = false;
			m_playerStateSync?.Refresh();
			if (m_simulationService != null)
			{
				m_simulationService.SecondsPerGameDay = Mathf.Max(1f, secondsPerBusinessDay);
			}
			Refresh();
			LogPanelSnapshot("OnEnable");
		}

		private void OnDisable()
		{
			Unsubscribe();
			m_businessDayTimer = 0f;
			m_uiRefreshTimer = 0f;
			m_daySimulationInProgress = false;
		}

		private void Update()
		{
			if (m_simulationService != null)
			{
				m_simulationService.SecondsPerGameDay = Mathf.Max(1f, secondsPerBusinessDay);
			}

			m_uiRefreshTimer += Time.deltaTime;
			if (m_uiRefreshTimer >= Mathf.Max(0.1f, uiRefreshIntervalSeconds))
			{
				m_uiRefreshTimer = 0f;
				RefreshSelected();
			}

			if (!autoSimulateServerBusinessDay || m_daySimulationInProgress || m_actionFacade == null || m_runtimeService == null)
			{
				return;
			}

			float daySeconds = Mathf.Max(1f, secondsPerBusinessDay);
			m_businessDayTimer += Time.deltaTime;
			if (m_businessDayTimer < daySeconds)
			{
				return;
			}

			m_businessDayTimer = 0f;
			_ = SimulateBusinessDayAsync();
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
				m_playerStateSync = bootstrap.PlayerStateSync;
				m_definitions = bootstrap.BusinessDefinitionsRepository;
				m_gameData = bootstrap.GameDataRepository;
				m_businessCalculation = new BusinessCalculationService(m_definitions, m_gameData);
				m_simulationService = bootstrap.BusinessSimulationService;
				m_profileSync = bootstrap.ProfileSyncService;
			}
			else
			{
				m_profileSync = null;
				m_playerStateSync = null;
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

			if (businesses.Count == 0)
			{
				m_selectedLotId = null;
				OnBusinessSelected(null);
				return;
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

			List<string> cashierOptions = BuildCashierContactOptions(null)
				.Where(o => o != null && !string.IsNullOrWhiteSpace(o.id))
				.Select(o => $"{o.id}:{o.displayName}")
				.ToList();

			List<string> merchOptions = BuildMerchandiserContactOptions(null)
				.Where(o => o != null && !string.IsNullOrWhiteSpace(o.id))
				.Select(o => $"{o.id}:{o.displayName}")
				.ToList();

			List<string> logistOptions = BuildSupplierContactOptions(null)
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

			IReadOnlyCollection<string> knownContactIds =
				m_stateSync != null ? m_stateSync.GetKnownContacts() : new List<string>();

			PopulateDropdowns(business, business != null ? business.lotId : m_selectedLotId);

			BusinessRuntimeSimulationState simulation =
				m_simulationService != null && !string.IsNullOrWhiteSpace(m_selectedLotId)
					? m_simulationService.GetStateByLotId(m_selectedLotId)
					: null;
			int storageCapacity = m_businessCalculation != null ? m_businessCalculation.GetStorageCapacity(business) : 0;
			float expensesPerDay = CalculateExpensesPerDay(business);

			detailsView.SetBusiness(
				business,
				simulation,
				storageCapacity,
				expensesPerDay,
				business != null ? Mathf.Max(0, business.autoDeliveryPerDay) : 0,
				business != null ? ResolveLotDisplayName(business.lotId) : null,
				business != null ? ResolveBusinessTypeDisplayName(business.businessTypeId) : null,
				knownContactIds.Select(ResolveContactDisplayName),
				business != null ? ResolveContactDisplayName(business.hiredLogistContactId) : null,
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

		private async Task SimulateBusinessDayAsync()
		{
			if (m_daySimulationInProgress || m_actionFacade == null || m_runtimeService == null)
			{
				return;
			}

			m_daySimulationInProgress = true;
			try
			{
				List<BusinessInstanceSnapshot> businesses = m_runtimeService
					.GetBusinesses()
					.Where(b => b != null && !string.IsNullOrWhiteSpace(b.lotId))
					.ToList();

				foreach (BusinessInstanceSnapshot business in businesses)
				{
					await m_actionFacade.SimulateBusinessDay(business.lotId);
				}
			}
			finally
			{
				m_daySimulationInProgress = false;
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
			detailsView.SetBusinessTypeSelectionVisible(business != null && !string.IsNullOrWhiteSpace(selectedLot));
			detailsView.SetBusinessTypeOptions(
				business != null ? BuildBusinessTypeOptions(selectedLot) : new List<BusinessDetailsView.IdOption>(),
				business != null ? business.businessTypeId : null);

			List<BusinessDetailsView.IdOption> storageOptions = BuildEquipmentOptions(business, "storage").ToList();
			List<BusinessDetailsView.IdOption> cashDeskOptions = BuildEquipmentOptions(business, "cashdesk").ToList();
			List<BusinessDetailsView.IdOption> shelfOptions = BuildEquipmentOptions(business, "shelf").ToList();

			detailsView.SetStorageOptions(storageOptions,
				ResolveSelectedOptionId(storageOptions, detailsView.GetPendingStorageId(), business != null ? business.storageItemId : null));
			detailsView.SetCashDeskOptions(cashDeskOptions,
				ResolveSelectedOptionId(cashDeskOptions, detailsView.GetPendingCashDeskId(), business != null ? business.cashDeskItemId : null));
			detailsView.SetShelfOptions(shelfOptions,
				ResolveSelectedOptionId(shelfOptions, detailsView.GetPendingShelfId(), business != null ? business.shelfItemId : null));

			bool requiresMerch = business != null && m_definitions != null && m_definitions.RequiresMerchandiser(business.businessTypeId);
			List<BusinessDetailsView.IdOption> supplierOptions = BuildSupplierContactOptions(business).ToList();
			List<BusinessDetailsView.IdOption> cashierOptions = BuildCashierContactOptions(business).ToList();
			List<BusinessDetailsView.IdOption> merchOptions = requiresMerch
				? BuildMerchandiserContactOptions(business).ToList()
				: new List<BusinessDetailsView.IdOption>();

			detailsView.SetSupplierOptions(
				supplierOptions,
				ResolveSelectedOptionId(supplierOptions, detailsView.GetPendingSupplierId(), business != null ? business.hiredLogistContactId : null));
			detailsView.SetCashierOptions(
				cashierOptions,
				ResolveSelectedOptionId(cashierOptions, detailsView.GetPendingCashierId(), business != null ? business.hiredCashierContactId : null));
			detailsView.SetMerchandiserOptions(
				merchOptions,
				requiresMerch
					? ResolveSelectedOptionId(merchOptions, detailsView.GetPendingMerchandiserId(), business != null ? business.hiredMerchContactId : null)
					: null);
		}

		private IEnumerable<BusinessDetailsView.IdOption> BuildBusinessTypeOptions(string lotId)
		{
			var options = new List<BusinessDetailsView.IdOption>();
			if (m_gameData == null || m_definitions == null || string.IsNullOrWhiteSpace(lotId))
			{
				return options;
			}

			LotDefinitionData lot = m_gameData.GetLotById(lotId);
			if (lot == null || lot.allowedBusinessTypes == null)
			{
				return options;
			}

			foreach (string typeId in lot.allowedBusinessTypes)
			{
				BusinessTypeDefinitionData type = m_definitions.GetBusinessType(typeId);
				if (type == null || string.IsNullOrWhiteSpace(type.id))
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
					displayName = ResolveBusinessSelectionDisplayName(business)
				});
			}

			return options;
		}

		private IEnumerable<BusinessDetailsView.IdOption> BuildEquipmentOptions(
			BusinessInstanceSnapshot business,
			string category)
		{
			var options = new List<BusinessDetailsView.IdOption>();
			if (m_definitions == null || m_playerStateSync == null || string.IsNullOrWhiteSpace(category))
			{
				return options;
			}

			var blockedItems = new HashSet<string>();
			if (m_runtimeService != null)
			{
				foreach (BusinessInstanceSnapshot other in m_runtimeService.GetBusinesses())
				{
					if (other == null || business != null && other.instanceId == business.instanceId)
					{
						continue;
					}

					AddIfNotEmpty(blockedItems, other.storageItemId);
					AddIfNotEmpty(blockedItems, other.cashDeskItemId);
					AddIfNotEmpty(blockedItems, other.shelfItemId);
				}
			}

			string equippedItemId = GetEquippedItemId(business, category);
			if (!string.IsNullOrWhiteSpace(equippedItemId))
			{
				TraderItemDefinitionData equippedItem = m_definitions.GetTraderItem(equippedItemId);
				if (equippedItem != null && IsEquipmentCategoryMatch(equippedItem.category, category))
				{
					options.Add(new BusinessDetailsView.IdOption
					{
						id = equippedItem.id,
						displayName = !string.IsNullOrWhiteSpace(equippedItem.name) ? equippedItem.name : equippedItem.id
					});
				}
			}

			foreach (string itemId in m_playerStateSync.Items)
			{
				if (string.IsNullOrWhiteSpace(itemId) || blockedItems.Contains(itemId))
				{
					continue;
				}

				TraderItemDefinitionData item = m_definitions.GetTraderItem(itemId);
				if (item == null || !IsEquipmentCategoryMatch(item.category, category))
				{
					continue;
				}

				options.Add(new BusinessDetailsView.IdOption
				{
					id = item.id,
					displayName = !string.IsNullOrWhiteSpace(item.name) ? item.name : item.id
				});
			}

			return options;
		}

		private static string ResolveSelectedOptionId(
			IReadOnlyList<BusinessDetailsView.IdOption> options,
			string pendingId,
			string currentId)
		{
			string normalizedPending = NormalizeId(pendingId);
			string normalizedCurrent = NormalizeId(currentId);

			if (normalizedPending != normalizedCurrent)
			{
				if (!string.IsNullOrWhiteSpace(normalizedPending) && ContainsOption(options, normalizedPending))
				{
					return normalizedPending;
				}

				return null;
			}

			if (!string.IsNullOrWhiteSpace(normalizedPending) && ContainsOption(options, normalizedPending))
			{
				return normalizedPending;
			}

			return ContainsOption(options, normalizedCurrent) ? normalizedCurrent : null;
		}

		private static bool ContainsOption(IReadOnlyList<BusinessDetailsView.IdOption> options, string id)
		{
			if (options == null || string.IsNullOrWhiteSpace(id))
			{
				return false;
			}

			for (int i = 0; i < options.Count; i++)
			{
				BusinessDetailsView.IdOption option = options[i];
				if (option != null && NormalizeId(option.id) == id)
				{
					return true;
				}
			}

			return false;
		}

		private static string NormalizeId(string value)
		{
			return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
		}

		private static string GetEquippedItemId(BusinessInstanceSnapshot business, string category)
		{
			if (business == null || string.IsNullOrWhiteSpace(category))
			{
				return null;
			}

			if (string.Equals(category, "storage", System.StringComparison.OrdinalIgnoreCase))
			{
				return business.storageItemId;
			}

			if (string.Equals(category, "cashdesk", System.StringComparison.OrdinalIgnoreCase))
			{
				return business.cashDeskItemId;
			}

			if (string.Equals(category, "shelf", System.StringComparison.OrdinalIgnoreCase))
			{
				return business.shelfItemId;
			}

			return null;
		}

		private static bool IsEquipmentCategoryMatch(string itemCategory, string slotCategory)
		{
			string normalizedItem = NormalizeEquipmentCategory(itemCategory);
			string normalizedSlot = NormalizeEquipmentCategory(slotCategory);
			return !string.IsNullOrWhiteSpace(normalizedItem) &&
			       !string.IsNullOrWhiteSpace(normalizedSlot) &&
			       string.Equals(normalizedItem, normalizedSlot, System.StringComparison.OrdinalIgnoreCase);
		}

		private static string NormalizeEquipmentCategory(string category)
		{
			if (string.IsNullOrWhiteSpace(category))
			{
				return null;
			}

			if (category.StartsWith("storage", System.StringComparison.OrdinalIgnoreCase))
			{
				return "storage";
			}

			if (category.StartsWith("cashdesk", System.StringComparison.OrdinalIgnoreCase))
			{
				return "cashdesk";
			}

			if (category.StartsWith("shelf", System.StringComparison.OrdinalIgnoreCase))
			{
				return "shelf";
			}

			return category.Trim();
		}

		private static void AddIfNotEmpty(HashSet<string> set, string value)
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				set.Add(value.Trim());
			}
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

		private IEnumerable<BusinessDetailsView.IdOption> BuildSupplierContactOptions(BusinessInstanceSnapshot business)
		{
			var options = new List<BusinessDetailsView.IdOption>();
			if (m_definitions == null || m_stateSync == null)
			{
				return options;
			}

			HashSet<string> known = new HashSet<string>(m_stateSync.GetKnownContacts());
			HashSet<string> knownSuppliers = new HashSet<string>();
			foreach (BusinessDetailsView.IdOption option in BuildSupplierOptions(business))
			{
				if (option != null && !string.IsNullOrWhiteSpace(option.id))
				{
					knownSuppliers.Add(option.id);
				}
			}

			foreach (StaffContactDefinitionData contact in m_definitions.GetAllStaffContacts())
			{
				if (contact == null || string.IsNullOrWhiteSpace(contact.id))
				{
					continue;
				}

				string id = contact.id.Trim().ToLowerInvariant();
				bool isSupplier = knownSuppliers.Contains(contact.id) || id.Contains("supplier") || id.Contains("logist");
				if (!isSupplier)
				{
					continue;
				}

				bool isCurrentAssigned = business != null &&
				                         string.Equals(contact.id, business.hiredLogistContactId, StringComparison.Ordinal);
				if (!known.Contains(contact.id) && !isCurrentAssigned)
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

		private IEnumerable<BusinessDetailsView.IdOption> BuildCashierContactOptions(BusinessInstanceSnapshot business)
		{
			return BuildWorkerContactOptionsByKeyword("cashier", business != null ? business.hiredCashierContactId : null);
		}

		private IEnumerable<BusinessDetailsView.IdOption> BuildMerchandiserContactOptions(BusinessInstanceSnapshot business)
		{
			return BuildWorkerContactOptionsByKeyword("merch", business != null ? business.hiredMerchContactId : null);
		}

		private IEnumerable<BusinessDetailsView.IdOption> BuildWorkerContactOptionsByKeyword(string keyword, string forceIncludeContactId)
		{
			var options = new List<BusinessDetailsView.IdOption>();
			if (m_definitions == null || m_stateSync == null || string.IsNullOrWhiteSpace(keyword))
			{
				return options;
			}

			HashSet<string> known = new HashSet<string>(m_stateSync.GetKnownContacts());
			string normalizedKeyword = keyword.Trim().ToLowerInvariant();
			foreach (StaffContactDefinitionData contact in m_definitions.GetAllStaffContacts())
			{
				if (contact == null || string.IsNullOrWhiteSpace(contact.id))
				{
					continue;
				}

				if (!contact.id.Trim().ToLowerInvariant().Contains(normalizedKeyword))
				{
					continue;
				}

				if (!known.Contains(contact.id) && !string.Equals(contact.id, forceIncludeContactId, StringComparison.Ordinal))
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

		private string ResolveLotDisplayName(string lotId)
		{
			if (string.IsNullOrWhiteSpace(lotId))
			{
				return "-";
			}

			LotDefinitionData lot = m_gameData != null ? m_gameData.GetLotById(lotId) : null;
			return lot != null && !string.IsNullOrWhiteSpace(lot.displayName) ? lot.displayName : lotId;
		}

		private string ResolveBusinessSelectionDisplayName(BusinessInstanceSnapshot business)
		{
			if (business == null)
			{
				return "-";
			}

			if (!string.IsNullOrWhiteSpace(business.businessTypeId))
			{
				return ResolveBusinessTypeDisplayName(business.businessTypeId);
			}

			return ResolveLotDisplayName(business.lotId);
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

		private float CalculateExpensesPerDay(BusinessInstanceSnapshot business)
		{
			if (business == null)
			{
				return 0f;
			}
			return m_businessCalculation != null ? m_businessCalculation.GetExpensesPerDay(business) : 0f;
		}
	}
}
