using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BusinessDetailsView : MonoBehaviour
{
    [Serializable]
    public class IdOption
    {
        public string id;
        public string displayName;
    }

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

    public event Action RentClicked;
    public event Action AssignTypeClicked;
    public event Action InstallModuleClicked;
    public event Action AssignSupplierClicked;
    public event Action HireWorkerClicked;
    public event Action OpenClicked;
    public event Action CloseClicked;
    public event Action SetMarkupClicked;
    public event Action UnlockContactClicked;
    public event Action RoleChanged;

    private BusinessInstanceSnapshot currentBusiness;
    private readonly List<IdOption> lotOptions = new List<IdOption>();
    private readonly List<IdOption> businessTypeOptions = new List<IdOption>();
    private readonly List<IdOption> moduleOptions = new List<IdOption>();
    private readonly List<IdOption> supplierOptions = new List<IdOption>();
    private readonly List<IdOption> roleOptions = new List<IdOption>();
    private readonly List<IdOption> workerContactOptions = new List<IdOption>();
    private readonly List<IdOption> unlockContactOptions = new List<IdOption>();

    private void Awake()
    {
        HookButton(rentButton, () => RentClicked?.Invoke());
        HookButton(assignTypeButton, () => AssignTypeClicked?.Invoke());
        HookButton(installModuleButton, () => InstallModuleClicked?.Invoke());
        HookButton(assignSupplierButton, () => AssignSupplierClicked?.Invoke());
        HookButton(hireWorkerButton, () => HireWorkerClicked?.Invoke());
        HookButton(openButton, () => OpenClicked?.Invoke());
        HookButton(closeButton, () => CloseClicked?.Invoke());
        HookButton(setMarkupButton, () => SetMarkupClicked?.Invoke());
        HookButton(unlockContactButton, () => UnlockContactClicked?.Invoke());

        if (roleDropdown != null)
        {
            roleDropdown.onValueChanged.AddListener(_ => RoleChanged?.Invoke());
        }
    }

    private static void HookButton(Button button, Action action)
    {
        if (button == null || action == null) return;
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
        currentBusiness = business;

        SetText(lotIdText, !string.IsNullOrWhiteSpace(lotDisplayName) ? lotDisplayName : (business != null ? business.lotId : "-"));
        SetText(businessTypeText, !string.IsNullOrWhiteSpace(businessTypeDisplayName) ? businessTypeDisplayName : (business != null ? business.businessTypeId : "-"));
        SetText(isOpenText, business != null ? (business.isOpen ? "Open" : "Closed") : "-");
        SetText(rentPerDayText, business != null ? business.rentPerDay.ToString() : "-");

        SetText(requiredModulesText, requiredModules != null ? string.Join(", ", requiredModules) : "-");
        SetText(installedModulesText, business != null && business.installedModules != null ? string.Join(", ", business.installedModules) : "-");
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

        SetText(knownContactsText, knownContactDisplayNames != null ? string.Join(", ", knownContactDisplayNames) : "-");
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
        var selectedLot = GetSelectedId(lotDropdown, lotOptions);
        if (!string.IsNullOrWhiteSpace(selectedLot))
        {
            return selectedLot;
        }

        if (currentBusiness != null && !string.IsNullOrWhiteSpace(currentBusiness.lotId))
        {
            return currentBusiness.lotId;
        }
        return lotIdInput != null ? lotIdInput.text : null;
    }

    public string GetBusinessTypeId() => GetSelectedId(businessTypeDropdown, businessTypeOptions) ?? (businessTypeIdInput != null ? businessTypeIdInput.text : null);
    public string GetModuleId() => GetSelectedId(moduleDropdown, moduleOptions) ?? (moduleIdInput != null ? moduleIdInput.text : null);
    public string GetSupplierId() => GetSelectedId(supplierDropdown, supplierOptions) ?? (supplierIdInput != null ? supplierIdInput.text : null);
    public string GetRoleId() => GetSelectedId(roleDropdown, roleOptions) ?? (roleIdInput != null ? roleIdInput.text : null);
    public string GetContactId() => GetSelectedId(workerContactDropdown, workerContactOptions) ?? (contactIdInput != null ? contactIdInput.text : null);
    public string GetUnlockContactId() => GetSelectedId(unlockContactDropdown, unlockContactOptions) ?? (contactIdInput != null ? contactIdInput.text : null);

    public int GetMarkupPercent()
    {
        if (markupInput == null) return 0;
        return int.TryParse(markupInput.text, out var value) ? value : 0;
    }

    public int GetRentPerDay()
    {
        if (rentInput == null) return 0;
        return int.TryParse(rentInput.text, out var value) ? value : 0;
    }

    public void SetLotOptions(IEnumerable<IdOption> options, string selectedId)
    {
        SetOptions(lotDropdown, lotOptions, options, selectedId, "Нет доступных помещений");
    }

    public void SetBusinessTypeOptions(IEnumerable<IdOption> options, string selectedId)
    {
        SetOptions(businessTypeDropdown, businessTypeOptions, options, selectedId, "Нет доступных типов");
    }

    public void SetModuleOptions(IEnumerable<IdOption> options, string selectedId)
    {
        SetOptions(moduleDropdown, moduleOptions, options, selectedId, "Нет доступных модулей");
    }

    public void SetSupplierOptions(IEnumerable<IdOption> options, string selectedId)
    {
        SetOptions(supplierDropdown, supplierOptions, options, selectedId, "Нет доступных поставщиков");
    }

    public void SetRoleOptions(IEnumerable<IdOption> options, string selectedId)
    {
        SetOptions(roleDropdown, roleOptions, options, selectedId, "Нет доступных ролей");
    }

    public void SetWorkerContactOptions(IEnumerable<IdOption> options, string selectedId)
    {
        SetOptions(workerContactDropdown, workerContactOptions, options, selectedId, "Нет доступных сотрудников");
    }

    public void SetUnlockContactOptions(IEnumerable<IdOption> options, string selectedId)
    {
        SetOptions(unlockContactDropdown, unlockContactOptions, options, selectedId, "Нет доступных контактов");
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
        int selectedIndex = 0;
        for (int i = 0; i < buffer.Count; i++)
        {
            var option = buffer[i];
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
}
