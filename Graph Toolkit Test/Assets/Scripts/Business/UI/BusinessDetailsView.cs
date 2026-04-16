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
	public class BusinessDetailsView : MonoBehaviour
	{
		[Header("Summary")]
		public TMP_Text lotIdText;

		public TMP_Text businessTypeText;
		public TMP_Text isOpenText;
		public TMP_Text rentPerDayText;

		[Header("Modules")]
		public TMP_Text requiredModulesText;

		public TMP_Text installedModulesText;
		public TMP_Text missingModulesText;

		[Header("Economy")]
		public TMP_Text markupText;

		public TMP_Text storageStockText;
		public TMP_Text shelfStockText;
		public TMP_Text incomeText;
		public TMP_Text expensesText;
		public TMP_Text profitText;

		[Header("Staff & Supplier")]
		public TMP_Text supplierText;

		public TMP_Text cashierText;
		public TMP_Text merchText;

		[Header("Contacts")]
		public TMP_Text knownContactsText;

		[Header("Inputs")]
		public TMP_InputField lotIdInput;

		public TMP_InputField businessTypeIdInput;
		public TMP_InputField moduleIdInput;
		public TMP_InputField supplierIdInput;
		public TMP_InputField roleIdInput;
		public TMP_InputField contactIdInput;
		public TMP_InputField markupInput;
		public TMP_InputField rentInput;

		[Header("Dropdowns (Player Facing)")]
		public TMP_Dropdown lotDropdown;

		public TMP_Dropdown businessTypeDropdown;
		public TMP_Dropdown moduleDropdown;
		public TMP_Dropdown supplierDropdown;
		public TMP_Dropdown roleDropdown;
		public TMP_Dropdown workerContactDropdown;
		public TMP_Dropdown unlockContactDropdown;

		[Header("Buttons")]
		public Button rentButton;

		public Button assignTypeButton;
		public Button installModuleButton;
		public Button assignSupplierButton;
		public Button hireWorkerButton;
		public Button openButton;
		public Button closeButton;
		public Button setMarkupButton;
		public Button unlockContactButton;
		private readonly List<IdOption> m_businessTypeOptions = new();
		private readonly List<IdOption> m_lotOptions = new();
		private readonly List<IdOption> m_moduleOptions = new();
		private readonly List<IdOption> m_roleOptions = new();
		private readonly List<IdOption> m_supplierOptions = new();
		private readonly List<IdOption> m_unlockContactOptions = new();
		private readonly List<IdOption> m_workerContactOptions = new();

		private BusinessInstanceSnapshot m_currentBusiness;

		private void Awake()
		{
			HookButton(rentButton, () => rentClicked?.Invoke());
			HookButton(assignTypeButton, () => assignTypeClicked?.Invoke());
			HookButton(installModuleButton, () => installModuleClicked?.Invoke());
			HookButton(assignSupplierButton, () => assignSupplierClicked?.Invoke());
			HookButton(hireWorkerButton, () => hireWorkerClicked?.Invoke());
			HookButton(openButton, () => openClicked?.Invoke());
			HookButton(closeButton, () => closeClicked?.Invoke());
			HookButton(setMarkupButton, () => setMarkupClicked?.Invoke());
			HookButton(unlockContactButton, () => unlockContactClicked?.Invoke());

			if (roleDropdown != null)
			{
				roleDropdown.onValueChanged.AddListener(value => roleChanged?.Invoke());
			}
		}

		public event Action rentClicked;
		public event Action assignTypeClicked;
		public event Action installModuleClicked;
		public event Action assignSupplierClicked;
		public event Action hireWorkerClicked;
		public event Action openClicked;
		public event Action closeClicked;
		public event Action setMarkupClicked;
		public event Action unlockContactClicked;
		public event Action roleChanged;

		private static void HookButton(Button button, Action action)
		{
			if (button == null || action == null)
			{
				return;
			}

			button.onClick.AddListener(() => action());
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
			m_currentBusiness = business;

			SetText(lotIdText,
				!string.IsNullOrWhiteSpace(lotDisplayName) ? lotDisplayName : business != null ? business.lotId : "-");
			SetText(businessTypeText,
				!string.IsNullOrWhiteSpace(businessTypeDisplayName) ? businessTypeDisplayName :
				business != null ? business.businessTypeId : "-");
			SetText(isOpenText, business != null ? business.isOpen ? "Open" : "Closed" : "-");
			SetText(rentPerDayText, business != null ? business.rentPerDay.ToString() : "-");

			SetText(requiredModulesText, requiredModules != null ? string.Join(", ", requiredModules) : "-");
			SetText(installedModulesText,
				business != null && business.installedModules != null ? string.Join(", ", business.installedModules) : "-");
			SetText(missingModulesText, missingModules != null ? string.Join(", ", missingModules) : "-");

			SetText(markupText, business != null ? business.markupPercent.ToString() : "-");

			if (simulation != null)
			{
				SetText(storageStockText, simulation.storageStock.ToString("0.##"));
				SetText(shelfStockText, simulation.shelfStock.ToString("0.##"));
				SetText(incomeText, simulation.accumulatedIncome.ToString("0.##"));
				SetText(expensesText, simulation.accumulatedExpenses.ToString("0.##"));
				SetText(profitText, (simulation.accumulatedIncome - simulation.accumulatedExpenses).ToString("0.##"));
			}
			else
			{
				SetText(storageStockText, business != null ? business.storageStock.ToString() : "-");
				SetText(shelfStockText, business != null ? business.shelfStock.ToString() : "-");
				SetText(incomeText, "-");
				SetText(expensesText, "-");
				SetText(profitText, "-");
			}

			SetText(supplierText, string.IsNullOrWhiteSpace(supplierDisplayName) ? "-" : supplierDisplayName);
			SetText(cashierText, string.IsNullOrWhiteSpace(cashierDisplayName) ? "-" : cashierDisplayName);
			SetText(merchText, string.IsNullOrWhiteSpace(merchDisplayName) ? "-" : merchDisplayName);

			SetText(knownContactsText,
				knownContactDisplayNames != null ? string.Join(", ", knownContactDisplayNames) : "-");
		}

		private static void SetText(TMP_Text target, string value)
		{
			if (target != null)
			{
				target.text = value;
			}
		}

		public string GetLotId()
		{
			string selectedLot = GetSelectedId(lotDropdown, m_lotOptions);
			if (!string.IsNullOrWhiteSpace(selectedLot))
			{
				return selectedLot;
			}

			if (m_currentBusiness != null && !string.IsNullOrWhiteSpace(m_currentBusiness.lotId))
			{
				return m_currentBusiness.lotId;
			}

			return lotIdInput != null ? lotIdInput.text : null;
		}

		public string GetBusinessTypeId()
		{
			return GetSelectedId(businessTypeDropdown, m_businessTypeOptions) ??
			       (businessTypeIdInput != null ? businessTypeIdInput.text : null);
		}

		public string GetModuleId()
		{
			return GetSelectedId(moduleDropdown, m_moduleOptions) ?? (moduleIdInput != null ? moduleIdInput.text : null);
		}

		public string GetSupplierId()
		{
			return GetSelectedId(supplierDropdown, m_supplierOptions) ??
			       (supplierIdInput != null ? supplierIdInput.text : null);
		}

		public string GetRoleId()
		{
			return GetSelectedId(roleDropdown, m_roleOptions) ?? (roleIdInput != null ? roleIdInput.text : null);
		}

		public string GetContactId()
		{
			return GetSelectedId(workerContactDropdown, m_workerContactOptions) ??
			       (contactIdInput != null ? contactIdInput.text : null);
		}

		public string GetUnlockContactId()
		{
			return GetSelectedId(unlockContactDropdown, m_unlockContactOptions) ??
			       (contactIdInput != null ? contactIdInput.text : null);
		}

		public int GetMarkupPercent()
		{
			if (markupInput == null)
			{
				return 0;
			}

			return int.TryParse(markupInput.text, out int value) ? value : 0;
		}

		public int GetRentPerDay()
		{
			if (rentInput == null)
			{
				return 0;
			}

			return int.TryParse(rentInput.text, out int value) ? value : 0;
		}

		public void SetLotOptions(IEnumerable<IdOption> options, string selectedId)
		{
			SetOptions(lotDropdown, m_lotOptions, options, selectedId, "Нет доступных помещений");
		}

		public void SetBusinessTypeOptions(IEnumerable<IdOption> options, string selectedId)
		{
			SetOptions(businessTypeDropdown, m_businessTypeOptions, options, selectedId, "Нет доступных типов");
		}

		public void SetModuleOptions(IEnumerable<IdOption> options, string selectedId)
		{
			SetOptions(moduleDropdown, m_moduleOptions, options, selectedId, "Нет доступных модулей");
		}

		public void SetSupplierOptions(IEnumerable<IdOption> options, string selectedId)
		{
			SetOptions(supplierDropdown, m_supplierOptions, options, selectedId, "Нет доступных поставщиков");
		}

		public void SetRoleOptions(IEnumerable<IdOption> options, string selectedId)
		{
			SetOptions(roleDropdown, m_roleOptions, options, selectedId, "Нет доступных ролей");
		}

		public void SetWorkerContactOptions(IEnumerable<IdOption> options, string selectedId)
		{
			SetOptions(workerContactDropdown, m_workerContactOptions, options, selectedId, "Нет доступных сотрудников");
		}

		public void SetUnlockContactOptions(IEnumerable<IdOption> options, string selectedId)
		{
			SetOptions(unlockContactDropdown, m_unlockContactOptions, options, selectedId, "Нет доступных контактов");
		}

		private static string GetSelectedId(TMP_Dropdown dropdown, List<IdOption> options)
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

		private static void SetOptions(
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
				buffer.AddRange(options.Where(o => o != null && !string.IsNullOrWhiteSpace(o.id)));
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
				return;
			}

			var labels = new List<string>(buffer.Count);
			var selectedIndex = 0;
			for (var i = 0; i < buffer.Count; i++)
			{
				IdOption option = buffer[i];
				labels.Add(string.IsNullOrWhiteSpace(option.displayName) ? option.id : option.displayName);
				if (!string.IsNullOrWhiteSpace(selectedId) && option.id == selectedId)
				{
					selectedIndex = i;
				}
			}

			dropdown.AddOptions(labels);
			dropdown.value = selectedIndex;
			dropdown.RefreshShownValue();
		}

		[Serializable]
		public class IdOption
		{
			public string id;
			public string displayName;
		}
	}
}