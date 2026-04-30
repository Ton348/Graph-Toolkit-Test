using System;
using System.Collections.Generic;
using Prototype.Business.Runtime;
using Prototype.Business.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Prototype.Business.UI
{
	public sealed class BusinessDetailsView : MonoBehaviour
	{
		public enum TabType
		{
			Overview,
			Setup,
			Staff
		}

		[Header("Window")]
		[SerializeField]
		private TMP_Text titleText;

		[SerializeField]
		private Button closeButton;

		[Header("Top Bar")]
		[SerializeField]
		private ContactSelectView businessSelectView;

		[SerializeField]
		private ContactSelectView businessTypeSelectView;

		[SerializeField]
		private Button openCloseButton;

		[SerializeField]
		private TMP_Text openCloseButtonText;

		[Header("Tabs")]
		[SerializeField]
		private Button overviewTabButton;

		[SerializeField]
		private Button setupTabButton;

		[SerializeField]
		private Button staffTabButton;

		[SerializeField]
		private GameObject overviewTabRoot;

		[SerializeField]
		private GameObject setupTabRoot;

		[SerializeField]
		private GameObject staffTabRoot;

		[SerializeField]
		private GameObject tabsRoot;

		[SerializeField]
		private GameObject tabContentRoot;

		[Header("Overview")]
		[SerializeField]
		private TMP_Text incomeValueText;

		[SerializeField]
		private TMP_Text expensesValueText;

		[SerializeField]
		private TMP_Text profitValueText;

		[SerializeField]
		private Button collectProfitButton;

		[SerializeField]
		private Slider priceSlider;

		[SerializeField]
		private TMP_Text priceValueText;

		[SerializeField]
		private TMP_Text warehouseStockText;

		[SerializeField]
		private Slider warehouseSlider;

		[SerializeField]
		private TMP_Text warehouseDailyValueText;

		[Header("Setup")]
		[SerializeField]
		private ContactSelectView storageSelectView;

		[SerializeField]
		private ContactSelectView cashDeskSelectView;

		[SerializeField]
		private ContactSelectView shelfSelectView;

		[Header("Staff")]
		[SerializeField]
		private ContactSelectView supplierSelectView;

		[SerializeField]
		private ContactSelectView cashierSelectView;

		[SerializeField]
		private ContactSelectView merchandiserSelectView;

		private BusinessInstanceSnapshot m_currentBusiness;
		private string m_currentLotId;
		private bool m_isBusinessOpen;
		private GameObject m_merchandiserRowRoot;
		private GameObject m_storageRowRoot;
		private GameObject m_cashDeskRowRoot;
		private GameObject m_shelfRowRoot;
		private TabType m_currentTab = TabType.Overview;

		private int m_pendingPrice;
		private string m_pendingBusinessTypeId;
		private string m_pendingStorageId;
		private string m_pendingCashDeskId;
		private string m_pendingShelfId;
		private string m_pendingSupplierId;
		private string m_pendingCashierId;
		private string m_pendingMerchandiserId;
		private int m_pendingAutoDeliveryPerDay;

		public event Action closeClicked;
		public event Action<string> businessChanged;
		public event Action<string> businessTypeChanged;
		public event Action openCloseClicked;
		public event Action collectProfitClicked;
		public event Action<TabType> tabChanged;

		private void Awake()
		{
			TryUpdatePremisesLabel();
			TryResolveBusinessSelectViews();
			TryResolveWarehouseRowBindings();
			TryResolveProfitRowBindings();
			TryResolveTabRoots();
			TryResolveSetupRows();
			TryResolveStaffRows();

			HookButton(closeButton, () => closeClicked?.Invoke());
			HookButton(openCloseButton, () => openCloseClicked?.Invoke());
			HookButton(collectProfitButton, () => collectProfitClicked?.Invoke());
			HookButton(overviewTabButton, () => SetTab(TabType.Overview));
			HookButton(setupTabButton, () => SetTab(TabType.Setup));
			HookButton(staffTabButton, () => SetTab(TabType.Staff));

			if (businessSelectView != null)
			{
				businessSelectView.selectionChanged += value => businessChanged?.Invoke(NormalizeId(value));
			}

			if (businessTypeSelectView != null)
			{
				businessTypeSelectView.selectionChanged += value =>
				{
					m_pendingBusinessTypeId = NormalizeId(value);
					businessTypeChanged?.Invoke(m_pendingBusinessTypeId);
				};
			}

			if (priceSlider != null)
			{
				priceSlider.wholeNumbers = true;
				priceSlider.minValue = 0f;
				priceSlider.maxValue = 100f;
				priceSlider.onValueChanged.AddListener(OnPriceChanged);
			}

			if (warehouseSlider != null)
			{
				warehouseSlider.wholeNumbers = true;
				warehouseSlider.onValueChanged.AddListener(OnWarehouseDailyAmountChanged);
			}

			HookContactSelectView(storageSelectView, value => m_pendingStorageId = value);
			HookContactSelectView(cashDeskSelectView, value => m_pendingCashDeskId = value);
			HookContactSelectView(shelfSelectView, value => m_pendingShelfId = value);
			HookContactSelectView(supplierSelectView, value => m_pendingSupplierId = value);
			HookContactSelectView(cashierSelectView, value => m_pendingCashierId = value);
			HookContactSelectView(merchandiserSelectView, value => m_pendingMerchandiserId = value);

			SetTab(TabType.Overview);
			SetBusinessTypeSelectedState(false);
			SetBusinessTypeSelectionVisible(false);
			SetMerchandiserVisible(false);
			SetBusinessOpenState(false);
			UpdatePriceText(0);
			UpdateWarehouseDailyAmountText(0);
			SetWarehouseStock(0, 0);
		}

		private void OnEnable()
		{
			m_currentLotId = null;
		}

		public void SetTab(TabType tab)
		{
			m_currentTab = tab;

			if (overviewTabRoot != null)
			{
				overviewTabRoot.SetActive(tab == TabType.Overview);
			}

			if (setupTabRoot != null)
			{
				setupTabRoot.SetActive(tab == TabType.Setup);
			}

			if (staffTabRoot != null)
			{
				staffTabRoot.SetActive(tab == TabType.Staff);
			}

			tabChanged?.Invoke(tab);
		}

		public void SetBusinessTypeSelectedState(bool hasBusinessType)
		{
			if (tabsRoot != null)
			{
				tabsRoot.SetActive(hasBusinessType);
			}

			if (tabContentRoot != null)
			{
				tabContentRoot.SetActive(hasBusinessType);
			}

			if (overviewTabButton != null)
			{
				overviewTabButton.gameObject.SetActive(hasBusinessType);
			}

			if (setupTabButton != null)
			{
				setupTabButton.gameObject.SetActive(hasBusinessType);
			}

			if (staffTabButton != null)
			{
				staffTabButton.gameObject.SetActive(hasBusinessType);
			}

			if (!hasBusinessType)
			{
				if (overviewTabRoot != null)
				{
					overviewTabRoot.SetActive(false);
				}

				if (setupTabRoot != null)
				{
					setupTabRoot.SetActive(false);
				}

				if (staffTabRoot != null)
				{
					staffTabRoot.SetActive(false);
				}
			}
			else
			{
				SetTab(m_currentTab);
			}
		}

		public void SetBusinessTypeSelectionVisible(bool visible)
		{
			GameObject businessTypeRowRoot = ResolveBusinessTypeRowRoot();
			if (businessTypeRowRoot != null)
			{
				businessTypeRowRoot.SetActive(visible);
			}

			if (businessTypeSelectView != null)
			{
				businessTypeSelectView.gameObject.SetActive(visible);
			}

			if (!visible)
			{
				m_pendingBusinessTypeId = null;
				if (businessTypeSelectView != null)
				{
					businessTypeSelectView.Setup(new List<ContactSelectOption>(), null, true, "Нет доступных бизнесов");
				}
			}
		}

		public void SetBusinessOpenState(bool isOpen)
		{
			m_isBusinessOpen = isOpen;
			if (openCloseButtonText != null)
			{
				openCloseButtonText.text = isOpen ? "Закрыть" : "Открыть";
			}

			if (openCloseButton != null && openCloseButton.image != null)
			{
				Color baseColor = isOpen
					? new Color(0.85f, 0.2f, 0.2f, 1f)
					: new Color(0.17f, 0.8f, 0.25f, 1f);
				openCloseButton.image.color = baseColor;

				ColorBlock colors = openCloseButton.colors;
				colors.normalColor = baseColor;
				colors.highlightedColor = Color.Lerp(baseColor, Color.white, 0.12f);
				colors.pressedColor = Color.Lerp(baseColor, Color.black, 0.18f);
				colors.selectedColor = colors.highlightedColor;
				colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.7f);
				colors.colorMultiplier = 1f;
				openCloseButton.colors = colors;
			}
		}

		public bool IsBusinessOpen()
		{
			return m_isBusinessOpen;
		}

		public void SetBusiness(
			BusinessInstanceSnapshot business,
			BusinessRuntimeSimulationState simulation,
			int storageCapacity,
			float expensesPerDay,
			int dailyOrderAmount,
			string lotDisplayName,
			string businessTypeDisplayName,
			IEnumerable<string> knownContactDisplayNames,
			string supplierDisplayName,
			string cashierDisplayName,
			string merchDisplayName)
		{
			string nextLotId = business != null ? NormalizeId(business.lotId) : null;
			bool isNewSelection = !string.Equals(m_currentLotId, nextLotId, StringComparison.Ordinal);
			m_currentLotId = nextLotId;
			m_currentBusiness = business;
			SetBusinessOpenState(business != null && business.isOpen);
			if (business == null)
			{
				if (tabsRoot != null)
				{
					tabsRoot.SetActive(false);
				}

				if (tabContentRoot != null)
				{
					tabContentRoot.SetActive(false);
				}
			}
			SetBusinessTypeSelectedState(business != null && !string.IsNullOrWhiteSpace(business.businessTypeId));
			SetBusinessTypeSelectionVisible(business != null && string.IsNullOrWhiteSpace(business.businessTypeId));
			SetSetupVisible(business != null && !string.IsNullOrWhiteSpace(business.businessTypeId));
			SetMerchandiserVisible(business != null && business.businessTypeId == "grocery_store");

			if (titleText != null)
			{
				string lotTitle = !string.IsNullOrWhiteSpace(lotDisplayName)
					? lotDisplayName
					: business != null
						? business.lotId
						: string.Empty;
				titleText.text = string.IsNullOrWhiteSpace(lotTitle) ? "Управление бизнесом" : $"Ваше помещение: {lotTitle}";
			}

			float income = business != null ? Mathf.Max(0, business.lastDayRevenue) : 0f;
			float expenses = business != null ? Mathf.Max(0, business.lastDayExpenses) : expensesPerDay;
			float profit = business != null ? business.totalProfit : income - expenses;
			SetIncome(income);
			SetExpenses(expenses);
			SetProfit(profit);

			int safeStock = simulation != null
				? Mathf.Max(0, Mathf.RoundToInt(simulation.storageStock))
				: business != null
					? Mathf.Max(0, business.storageStock)
					: 0;
			int safeCapacity = Mathf.Max(0, storageCapacity);
			SetWarehouseStock(safeStock, safeCapacity);

			if (isNewSelection && business != null)
			{
				m_pendingBusinessTypeId = NormalizeId(business.businessTypeId);
				m_pendingPrice = business.markupPercent;
				m_pendingAutoDeliveryPerDay = Mathf.Max(0, dailyOrderAmount);
			}
			else if (isNewSelection)
			{
				m_pendingBusinessTypeId = null;
				m_pendingPrice = 0;
				m_pendingAutoDeliveryPerDay = 0;
			}
			if (priceSlider != null)
			{
				float clamped = Mathf.Clamp(m_pendingPrice, priceSlider.minValue, priceSlider.maxValue);
				priceSlider.SetValueWithoutNotify(clamped);
				m_pendingPrice = Mathf.RoundToInt(clamped);
			}

			UpdatePriceText(m_pendingPrice);
			RefreshWarehouseSliderRange(storageCapacity);
			if (warehouseSlider != null)
			{
				float clampedDaily = Mathf.Clamp(m_pendingAutoDeliveryPerDay, warehouseSlider.minValue, warehouseSlider.maxValue);
				warehouseSlider.SetValueWithoutNotify(clampedDaily);
				m_pendingAutoDeliveryPerDay = Mathf.RoundToInt(clampedDaily);
			}

			UpdateWarehouseDailyAmountText(m_pendingAutoDeliveryPerDay);

			if (isNewSelection && business != null)
			{
				m_pendingStorageId = NormalizeId(business.storageItemId);
				m_pendingCashDeskId = NormalizeId(business.cashDeskItemId);
				m_pendingShelfId = NormalizeId(business.shelfItemId);
				m_pendingSupplierId = NormalizeId(business.selectedSupplierId);
				m_pendingCashierId = NormalizeId(business.hiredCashierContactId);
				m_pendingMerchandiserId = NormalizeId(business.hiredMerchContactId);
			}
		}

		public void SetBusinessOptions(IEnumerable<IdOption> options, string selectedId)
		{
			SetContactOptions(businessSelectView, options, selectedId, false, "Нет помещений");
		}

		public void SetBusinessTypeOptions(IEnumerable<IdOption> options, string selectedId)
		{
			string normalizedSelectedId = NormalizeId(selectedId);
			string effectiveSelectedId = !string.IsNullOrWhiteSpace(normalizedSelectedId)
				? normalizedSelectedId
				: NormalizeId(m_pendingBusinessTypeId);

			if (!string.IsNullOrWhiteSpace(normalizedSelectedId))
			{
				m_pendingBusinessTypeId = normalizedSelectedId;
			}

			SetContactOptions(businessTypeSelectView, options, effectiveSelectedId, true, "Выберите тип бизнеса");
		}

		public string GetSelectedBusinessId()
		{
			if (businessSelectView != null)
			{
				string selectedFromView = businessSelectView.GetSelectedId();
				if (!string.IsNullOrWhiteSpace(selectedFromView))
				{
					return selectedFromView;
				}
			}

			if (m_currentBusiness != null && !string.IsNullOrWhiteSpace(m_currentBusiness.lotId))
			{
				return m_currentBusiness.lotId;
			}

			return null;
		}

		public string GetSelectedBusinessTypeId()
		{
			if (!string.IsNullOrWhiteSpace(m_pendingBusinessTypeId))
			{
				return NormalizeId(m_pendingBusinessTypeId);
			}

			return businessTypeSelectView != null
				? NormalizeId(businessTypeSelectView.GetSelectedId())
				: null;
		}

		public void SetStorageOptions(IEnumerable<IdOption> options, string selectedId)
		{
			SetContactOptionsWithNone(storageSelectView, options, selectedId, "Нет");
			m_pendingStorageId = NormalizeId(storageSelectView != null ? storageSelectView.GetSelectedId() : selectedId);
		}

		public void SetCashDeskOptions(IEnumerable<IdOption> options, string selectedId)
		{
			SetContactOptionsWithNone(cashDeskSelectView, options, selectedId, "Нет");
			m_pendingCashDeskId = NormalizeId(cashDeskSelectView != null ? cashDeskSelectView.GetSelectedId() : selectedId);
		}

		public void SetShelfOptions(IEnumerable<IdOption> options, string selectedId)
		{
			SetContactOptionsWithNone(shelfSelectView, options, selectedId, "Нет");
			m_pendingShelfId = NormalizeId(shelfSelectView != null ? shelfSelectView.GetSelectedId() : selectedId);
		}

		public void SetSupplierOptions(IEnumerable<IdOption> options, string selectedId)
		{
			SetContactOptionsWithNone(supplierSelectView, options, selectedId, "Нет");
			m_pendingSupplierId = NormalizeId(supplierSelectView != null ? supplierSelectView.GetSelectedId() : selectedId);
		}

		public void SetCashierOptions(IEnumerable<IdOption> options, string selectedId)
		{
			SetContactOptionsWithNone(cashierSelectView, options, selectedId, "Нет");
			m_pendingCashierId = NormalizeId(cashierSelectView != null ? cashierSelectView.GetSelectedId() : selectedId);
		}

		public void SetMerchandiserOptions(IEnumerable<IdOption> options, string selectedId)
		{
			SetContactOptionsWithNone(merchandiserSelectView, options, selectedId, "Нет");
			m_pendingMerchandiserId = NormalizeId(merchandiserSelectView != null ? merchandiserSelectView.GetSelectedId() : selectedId);
		}

		public void SetIncome(float value)
		{
			if (incomeValueText != null)
			{
				incomeValueText.text = value.ToString("0.##");
			}
		}

		public void SetExpenses(float value)
		{
			if (expensesValueText != null)
			{
				expensesValueText.text = value.ToString("0.##");
			}
		}

		public void SetProfit(float value)
		{
			if (profitValueText != null)
			{
				profitValueText.text = value.ToString("0.##");
			}

			if (collectProfitButton != null)
			{
				collectProfitButton.interactable = value > 0f;
			}
		}

		public int GetPendingPrice()
		{
			return m_pendingPrice;
		}

		public string GetPendingStorageId()
		{
			return m_pendingStorageId;
		}

		public string GetPendingCashDeskId()
		{
			return m_pendingCashDeskId;
		}

		public string GetPendingShelfId()
		{
			return m_pendingShelfId;
		}

		public string GetPendingSupplierId()
		{
			return m_pendingSupplierId;
		}

		public string GetPendingCashierId()
		{
			return m_pendingCashierId;
		}

		public string GetPendingMerchandiserId()
		{
			return m_pendingMerchandiserId;
		}

		public int GetPendingAutoDeliveryPerDay()
		{
			return m_pendingAutoDeliveryPerDay;
		}

		private void OnPriceChanged(float value)
		{
			m_pendingPrice = Mathf.Clamp(Mathf.RoundToInt(value), 0, 100);
			UpdatePriceText(m_pendingPrice);
		}

		private void OnWarehouseDailyAmountChanged(float value)
		{
			m_pendingAutoDeliveryPerDay = Mathf.Max(0, Mathf.RoundToInt(value));
			UpdateWarehouseDailyAmountText(m_pendingAutoDeliveryPerDay);
		}

		private static void HookButton(Button button, Action handler)
		{
			if (button == null || handler == null)
			{
				return;
			}

			button.onClick.AddListener(() => handler());
		}

		private static void HookContactSelectView(ContactSelectView view, Action<string> setValue)
		{
			if (view == null || setValue == null)
			{
				return;
			}

			view.selectionChanged += value => setValue(NormalizeId(value));
		}

		private static void SetContactOptionsWithNone(
			ContactSelectView view,
			IEnumerable<IdOption> options,
			string selectedId,
			string noneLabel)
		{
			if (view == null)
			{
				return;
			}

			var list = new List<ContactSelectOption>();
			if (options != null)
			{
				foreach (IdOption option in options)
				{
					if (option == null)
					{
						continue;
					}

					list.Add(new ContactSelectOption
					{
						id = option.id,
						displayName = option.displayName
					});
				}
			}

			view.Setup(list, selectedId, true, noneLabel);
		}

		private static void SetContactOptions(
			ContactSelectView view,
			IEnumerable<IdOption> options,
			string selectedId,
			bool includeNone,
			string noneLabel)
		{
			if (view == null)
			{
				return;
			}

			var list = new List<ContactSelectOption>();
			if (options != null)
			{
				foreach (IdOption option in options)
				{
					if (option == null)
					{
						continue;
					}

					list.Add(new ContactSelectOption
					{
						id = option.id,
						displayName = option.displayName
					});
				}
			}

			view.Setup(list, selectedId, includeNone, noneLabel);
		}

		private static string NormalizeId(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return null;
			}

			return value.Trim();
		}

		private void UpdatePriceText(int value)
		{
			if (priceValueText != null)
			{
				priceValueText.text = value.ToString();
			}
		}

		private void UpdateWarehouseDailyAmountText(int value)
		{
			if (warehouseDailyValueText != null)
			{
				warehouseDailyValueText.text = $"{Mathf.Max(0, value)} в день";
			}
		}

		private void SetWarehouseStock(int currentStock, int capacity)
		{
			if (warehouseStockText != null)
			{
				warehouseStockText.text = $"{Mathf.Max(0, currentStock)} / {Mathf.Max(0, capacity)}";
			}
		}

		private void RefreshWarehouseSliderRange(int storageCapacity)
		{
			if (warehouseSlider == null)
			{
				return;
			}

			int capacity = Mathf.Max(0, storageCapacity);
			warehouseSlider.wholeNumbers = true;
			warehouseSlider.minValue = 0f;
			warehouseSlider.maxValue = Mathf.Max(1, capacity);
		}

		private void TryResolveWarehouseRowBindings()
		{
			if (warehouseSlider != null && warehouseStockText != null && warehouseDailyValueText != null)
			{
				return;
			}

			Transform root = overviewTabRoot != null ? overviewTabRoot.transform : transform;
			Transform row = root != null ? root.Find("WarehouseRow") : null;
			if (row == null)
			{
				return;
			}

			if (warehouseSlider == null)
			{
				Transform sliderTransform = row.Find("PriceControls/PriceSlider");
				if (sliderTransform != null)
				{
					warehouseSlider = sliderTransform.GetComponent<Slider>();
				}
			}

			if (warehouseStockText == null)
			{
				Transform stockTransform = row.Find("PriceLabel");
				if (stockTransform != null)
				{
					warehouseStockText = stockTransform.GetComponent<TMP_Text>();
				}
			}

			if (warehouseDailyValueText == null)
			{
				Transform valueTransform = row.Find("PriceControls/PriceValueText");
				if (valueTransform != null)
				{
					warehouseDailyValueText = valueTransform.GetComponent<TMP_Text>();
				}
			}
		}

		private void TryResolveTabRoots()
		{
			if (tabsRoot == null)
			{
				Transform tabs = transform.Find("Tabs");
				if (tabs != null)
				{
					tabsRoot = tabs.gameObject;
				}
			}

			if (tabContentRoot == null)
			{
				Transform content = transform.Find("TabContentRoot");
				if (content != null)
				{
					tabContentRoot = content.gameObject;
				}
			}
		}

		private void TryUpdatePremisesLabel()
		{
			Transform anchor = businessSelectView != null ? businessSelectView.transform : null;
			Transform topBar = anchor != null ? anchor.parent : null;
			Transform labelTransform = topBar != null ? topBar.Find("BusinessLabel") : null;
			TMP_Text label = labelTransform != null ? labelTransform.GetComponent<TMP_Text>() : null;
			if (label != null)
			{
				label.text = "Помещение:";
			}
		}

		private void TryResolveBusinessSelectViews()
		{
			if (businessSelectView == null)
			{
				Transform topBar = transform.Find("TopBar");
				Transform businessView = topBar != null ? topBar.Find("BusinessSelectView") : null;
				if (businessView != null)
				{
					businessSelectView = businessView.GetComponent<ContactSelectView>();
				}
			}

			GameObject businessTypeRowRoot = ResolveBusinessTypeRowRoot();
			if (businessTypeSelectView == null && businessTypeRowRoot != null)
			{
				Transform typeView = businessTypeRowRoot.transform.Find("BusinessTypeSelectView");
				if (typeView != null)
				{
					businessTypeSelectView = typeView.GetComponent<ContactSelectView>();
				}
			}

			Transform setupTab = transform.Find("TabContentRoot/SetupTab");
			if (storageSelectView == null && setupTab != null)
			{
				Transform view = setupTab.Find("StorageRow/StorageSelectView");
				if (view != null)
				{
					storageSelectView = view.GetComponent<ContactSelectView>();
				}
			}

			if (cashDeskSelectView == null && setupTab != null)
			{
				Transform view = setupTab.Find("CashDeskRow/CashSelectView");
				if (view == null)
				{
					view = setupTab.Find("CashDeskRow/CashDeskSelectView");
				}
				if (view != null)
				{
					cashDeskSelectView = view.GetComponent<ContactSelectView>();
				}
			}

			if (shelfSelectView == null && setupTab != null)
			{
				Transform view = setupTab.Find("ShelfRow/ShelfSelectView");
				if (view != null)
				{
					shelfSelectView = view.GetComponent<ContactSelectView>();
				}
			}

			Transform staffTab = transform.Find("TabContentRoot/StaffTab");
			if (supplierSelectView == null && staffTab != null)
			{
				Transform view = staffTab.Find("SupplierRow/SupplierSelectView");
				if (view != null)
				{
					supplierSelectView = view.GetComponent<ContactSelectView>();
				}
			}

			if (cashierSelectView == null && staffTab != null)
			{
				Transform view = staffTab.Find("CashierRow/CashierSelectView");
				if (view != null)
				{
					cashierSelectView = view.GetComponent<ContactSelectView>();
				}
			}

			if (merchandiserSelectView == null && staffTab != null)
			{
				Transform view = staffTab.Find("MerchandiserRow/MerchandiserSelectView");
				if (view != null)
				{
					merchandiserSelectView = view.GetComponent<ContactSelectView>();
				}
			}
		}

		private void TryResolveSetupRows()
		{
			if (m_storageRowRoot == null)
			{
				Transform row = transform.Find("TabContentRoot/SetupTab/StorageRow");
				if (row != null)
				{
					m_storageRowRoot = row.gameObject;
				}
			}

			if (m_cashDeskRowRoot == null)
			{
				Transform row = transform.Find("TabContentRoot/SetupTab/CashDeskRow");
				if (row != null)
				{
					m_cashDeskRowRoot = row.gameObject;
				}
			}

			if (m_shelfRowRoot == null)
			{
				Transform row = transform.Find("TabContentRoot/SetupTab/ShelfRow");
				if (row != null)
				{
					m_shelfRowRoot = row.gameObject;
				}
			}
		}

		private void TryResolveStaffRows()
		{
			if (m_merchandiserRowRoot == null)
			{
				Transform merchRow = transform.Find("TabContentRoot/StaffTab/MerchandiserRow");
				if (merchRow != null)
				{
					m_merchandiserRowRoot = merchRow.gameObject;
				}
			}
		}

		private void SetMerchandiserVisible(bool visible)
		{
			if (m_merchandiserRowRoot != null)
			{
				m_merchandiserRowRoot.SetActive(visible);
			}
		}

		private void SetSetupVisible(bool visible)
		{
			if (m_storageRowRoot != null)
			{
				m_storageRowRoot.SetActive(visible);
			}

			if (m_cashDeskRowRoot != null)
			{
				m_cashDeskRowRoot.SetActive(visible);
			}

			if (m_shelfRowRoot != null)
			{
				m_shelfRowRoot.SetActive(visible);
			}
		}

		private GameObject ResolveBusinessTypeRowRoot()
		{
			Transform row = transform.Find("BusinessTypeRow");
			return row != null ? row.gameObject : null;
		}

		private void TryResolveProfitRowBindings()
		{
			if (collectProfitButton != null)
			{
				return;
			}

			Transform root = overviewTabRoot != null ? overviewTabRoot.transform : transform;
			Transform buttonTransform = root != null ? root.Find("take_money") : null;
			if (buttonTransform == null)
			{
				buttonTransform = root != null ? root.Find("ProfitRow/take_money") : null;
			}

			if (buttonTransform != null)
			{
				collectProfitButton = buttonTransform.GetComponent<Button>();
			}
		}

		[Serializable]
		public sealed class IdOption
		{
			public string id;
			public string displayName;
		}
	}
}
