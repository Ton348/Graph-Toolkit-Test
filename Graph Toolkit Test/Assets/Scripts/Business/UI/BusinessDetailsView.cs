using System;
using System.Collections.Generic;
using System.Linq;
using Prototype.Business.Runtime;
using Prototype.Business.Simulation;
using TMPro;
using UnityEngine;
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
		private TMP_Dropdown businessDropdown;

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

		[Header("Overview")]
		[SerializeField]
		private TMP_Text incomeValueText;

		[SerializeField]
		private TMP_Text expensesValueText;

		[SerializeField]
		private TMP_Text profitValueText;

		[SerializeField]
		private Slider priceSlider;

		[SerializeField]
		private TMP_Text priceValueText;

		[Header("Setup")]
		[SerializeField]
		private TMP_Dropdown storageDropdown;

		[SerializeField]
		private TMP_Dropdown cashDeskDropdown;

		[SerializeField]
		private TMP_Dropdown shelfDropdown;

		[Header("Staff")]
		[SerializeField]
		private TMP_Dropdown supplierDropdown;

		[SerializeField]
		private TMP_Dropdown cashierDropdown;

		[SerializeField]
		private TMP_Dropdown merchandiserDropdown;

		private readonly List<IdOption> m_businessOptions = new();
		private readonly List<IdOption> m_storageOptions = new();
		private readonly List<IdOption> m_cashDeskOptions = new();
		private readonly List<IdOption> m_shelfOptions = new();
		private readonly List<IdOption> m_supplierOptions = new();
		private readonly List<IdOption> m_cashierOptions = new();
		private readonly List<IdOption> m_merchandiserOptions = new();

		private BusinessInstanceSnapshot m_currentBusiness;
		private string m_currentLotId;
		private bool m_isBusinessOpen;
		private bool m_updatingBusinessDropdown;

		private int m_pendingPrice;
		private string m_pendingStorageId;
		private string m_pendingCashDeskId;
		private string m_pendingShelfId;
		private string m_pendingSupplierId;
		private string m_pendingCashierId;
		private string m_pendingMerchandiserId;

		public event Action closeClicked;
		public event Action<string> businessChanged;
		public event Action openCloseClicked;
		public event Action<TabType> tabChanged;

		private void Awake()
		{
			HookButton(closeButton, () => closeClicked?.Invoke());
			HookButton(openCloseButton, () => openCloseClicked?.Invoke());
			HookButton(overviewTabButton, () => SetTab(TabType.Overview));
			HookButton(setupTabButton, () => SetTab(TabType.Setup));
			HookButton(staffTabButton, () => SetTab(TabType.Staff));

			if (businessDropdown != null)
			{
				businessDropdown.onValueChanged.AddListener(OnBusinessDropdownChanged);
			}

			if (priceSlider != null)
			{
				priceSlider.onValueChanged.AddListener(OnPriceChanged);
			}

			HookPendingDropdown(storageDropdown, m_storageOptions, value => m_pendingStorageId = value);
			HookPendingDropdown(cashDeskDropdown, m_cashDeskOptions, value => m_pendingCashDeskId = value);
			HookPendingDropdown(shelfDropdown, m_shelfOptions, value => m_pendingShelfId = value);
			HookPendingDropdown(supplierDropdown, m_supplierOptions, value => m_pendingSupplierId = value);
			HookPendingDropdown(cashierDropdown, m_cashierOptions, value => m_pendingCashierId = value);
			HookPendingDropdown(merchandiserDropdown, m_merchandiserOptions, value => m_pendingMerchandiserId = value);

			SetTab(TabType.Overview);
			SetBusinessOpenState(false);
			UpdatePriceText(0);
		}

		public void SetTab(TabType tab)
		{
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

		public void SetBusinessOpenState(bool isOpen)
		{
			m_isBusinessOpen = isOpen;
			if (openCloseButtonText != null)
			{
				openCloseButtonText.text = isOpen ? "Закрыть" : "Открыть";
			}
		}

		public bool IsBusinessOpen()
		{
			return m_isBusinessOpen;
		}

		public void SetBusiness(
			BusinessInstanceSnapshot business,
			BusinessRuntimeSimulationState simulation,
			IEnumerable<string> requiredModules,
			IEnumerable<string> missingModules,
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

			if (titleText != null)
			{
				string lotTitle = !string.IsNullOrWhiteSpace(lotDisplayName)
					? lotDisplayName
					: business != null
						? business.lotId
						: string.Empty;
				titleText.text = string.IsNullOrWhiteSpace(lotTitle) ? "Управление бизнесом" : $"Ваш бизнес: {lotTitle}";
			}

			float income = simulation != null ? simulation.accumulatedIncome : 0f;
			float expenses = simulation != null ? simulation.accumulatedExpenses : 0f;
			float profit = income - expenses;
			SetIncome(income);
			SetExpenses(expenses);
			SetProfit(profit);

			if (isNewSelection && business != null)
			{
				m_pendingPrice = business.markupPercent;
			}
			if (priceSlider != null)
			{
				float clamped = Mathf.Clamp(m_pendingPrice, priceSlider.minValue, priceSlider.maxValue);
				priceSlider.SetValueWithoutNotify(clamped);
				m_pendingPrice = Mathf.RoundToInt(clamped);
			}

			UpdatePriceText(m_pendingPrice);

			m_pendingSupplierId = NormalizeId(business != null ? business.selectedSupplierId : m_pendingSupplierId);
			m_pendingCashierId = NormalizeId(business != null ? business.hiredCashierContactId : m_pendingCashierId);
			m_pendingMerchandiserId = NormalizeId(business != null ? business.hiredMerchContactId : m_pendingMerchandiserId);
		}

		public void SetBusinessOptions(IEnumerable<IdOption> options, string selectedId)
		{
			SetOptions(businessDropdown, m_businessOptions, options, selectedId, "Нет доступных бизнесов");
		}

		public string GetSelectedBusinessId()
		{
			string selected = GetSelectedId(businessDropdown, m_businessOptions);
			if (!string.IsNullOrWhiteSpace(selected))
			{
				return selected;
			}

			if (m_currentBusiness != null && !string.IsNullOrWhiteSpace(m_currentBusiness.lotId))
			{
				return m_currentBusiness.lotId;
			}

			return null;
		}

		public void SetStorageOptions(IEnumerable<IdOption> options, string selectedId)
		{
			SetOptionsWithNone(storageDropdown, m_storageOptions, options, selectedId, "Нет");
			m_pendingStorageId = NormalizeId(GetSelectedId(storageDropdown, m_storageOptions));
		}

		public void SetCashDeskOptions(IEnumerable<IdOption> options, string selectedId)
		{
			SetOptionsWithNone(cashDeskDropdown, m_cashDeskOptions, options, selectedId, "Нет");
			m_pendingCashDeskId = NormalizeId(GetSelectedId(cashDeskDropdown, m_cashDeskOptions));
		}

		public void SetShelfOptions(IEnumerable<IdOption> options, string selectedId)
		{
			SetOptionsWithNone(shelfDropdown, m_shelfOptions, options, selectedId, "Нет");
			m_pendingShelfId = NormalizeId(GetSelectedId(shelfDropdown, m_shelfOptions));
		}

		public void SetSupplierOptions(IEnumerable<IdOption> options, string selectedId)
		{
			SetOptionsWithNone(supplierDropdown, m_supplierOptions, options, selectedId, "Нет");
			m_pendingSupplierId = NormalizeId(GetSelectedId(supplierDropdown, m_supplierOptions));
		}

		public void SetCashierOptions(IEnumerable<IdOption> options, string selectedId)
		{
			SetOptionsWithNone(cashierDropdown, m_cashierOptions, options, selectedId, "Нет");
			m_pendingCashierId = NormalizeId(GetSelectedId(cashierDropdown, m_cashierOptions));
		}

		public void SetMerchandiserOptions(IEnumerable<IdOption> options, string selectedId)
		{
			SetOptionsWithNone(merchandiserDropdown, m_merchandiserOptions, options, selectedId, "Нет");
			m_pendingMerchandiserId = NormalizeId(GetSelectedId(merchandiserDropdown, m_merchandiserOptions));
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

		private void OnBusinessDropdownChanged(int value)
		{
			if (m_updatingBusinessDropdown)
			{
				return;
			}

			businessChanged?.Invoke(GetSelectedBusinessId());
		}

		private void OnPriceChanged(float value)
		{
			m_pendingPrice = Mathf.RoundToInt(value);
			UpdatePriceText(m_pendingPrice);
		}

		private static void HookButton(Button button, Action handler)
		{
			if (button == null || handler == null)
			{
				return;
			}

			button.onClick.AddListener(() => handler());
		}

		private static void HookPendingDropdown(TMP_Dropdown dropdown, List<IdOption> options, Action<string> setValue)
		{
			if (dropdown == null || options == null || setValue == null)
			{
				return;
			}

			dropdown.onValueChanged.AddListener(_ => setValue(NormalizeId(GetSelectedId(dropdown, options))));
		}

		private void SetOptionsWithNone(
			TMP_Dropdown dropdown,
			List<IdOption> buffer,
			IEnumerable<IdOption> options,
			string selectedId,
			string noneLabel)
		{
			var merged = new List<IdOption>
			{
				new IdOption
				{
					id = string.Empty,
					displayName = noneLabel
				}
			};

			if (options != null)
			{
				merged.AddRange(options.Where(o => o != null));
			}

			SetOptions(dropdown, buffer, merged, selectedId, noneLabel);
		}

		private void SetOptions(
			TMP_Dropdown dropdown,
			List<IdOption> buffer,
			IEnumerable<IdOption> options,
			string selectedId,
			string emptyLabel)
		{
			if (buffer == null)
			{
				return;
			}

			buffer.Clear();
			if (options != null)
			{
				foreach (IdOption option in options)
				{
					if (option == null)
					{
						continue;
					}

					buffer.Add(new IdOption
					{
						id = option.id,
						displayName = option.displayName
					});
				}
			}

			if (dropdown == null)
			{
				return;
			}

			dropdown.ClearOptions();
			if (buffer.Count == 0)
			{
				dropdown.AddOptions(new List<string> { emptyLabel });
				dropdown.value = 0;
				dropdown.RefreshShownValue();
				return;
			}

			int selectedIndex = 0;
			string normalizedSelectedId = NormalizeId(selectedId);
			var labels = new List<string>(buffer.Count);
			for (int i = 0; i < buffer.Count; i++)
			{
				IdOption option = buffer[i];
				labels.Add(string.IsNullOrWhiteSpace(option.displayName) ? option.id : option.displayName);
				if (normalizedSelectedId == NormalizeId(option.id))
				{
					selectedIndex = i;
				}
			}

			if (dropdown == businessDropdown)
			{
				m_updatingBusinessDropdown = true;
			}

			dropdown.AddOptions(labels);
			dropdown.value = Mathf.Clamp(selectedIndex, 0, labels.Count - 1);
			dropdown.RefreshShownValue();

			if (dropdown == businessDropdown)
			{
				m_updatingBusinessDropdown = false;
			}
		}

		private static string GetSelectedId(TMP_Dropdown dropdown, IReadOnlyList<IdOption> options)
		{
			if (dropdown == null || options == null || options.Count == 0)
			{
				return null;
			}

			int index = dropdown.value;
			if (index < 0 || index >= options.Count)
			{
				return null;
			}

			return options[index].id;
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

		[Serializable]
		public sealed class IdOption
		{
			public string id;
			public string displayName;
		}
	}
}
