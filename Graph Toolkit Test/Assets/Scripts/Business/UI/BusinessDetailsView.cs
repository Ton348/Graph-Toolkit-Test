using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private BusinessInstanceSnapshot currentBusiness;

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
    }

    private static void HookButton(Button button, Action action)
    {
        if (button == null || action == null) return;
        button.onClick.AddListener(() => action());
    }

    public void SetBusiness(BusinessInstanceSnapshot business, BusinessSimulationState simulation, IEnumerable<string> requiredModules, IEnumerable<string> missingModules, IEnumerable<string> knownContacts)
    {
        currentBusiness = business;

        SetText(lotIdText, business != null ? business.lotId : "-");
        SetText(businessTypeText, business != null ? business.businessTypeId : "-");
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

        SetText(supplierText, business != null ? business.selectedSupplierId : "-");
        SetText(cashierText, business != null ? business.hiredCashierContactId : "-");
        SetText(merchText, business != null ? business.hiredMerchContactId : "-");

        SetText(knownContactsText, knownContacts != null ? string.Join(", ", knownContacts) : "-");
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
        if (currentBusiness != null && !string.IsNullOrWhiteSpace(currentBusiness.lotId))
        {
            return currentBusiness.lotId;
        }
        return lotIdInput != null ? lotIdInput.text : null;
    }

    public string GetBusinessTypeId() => businessTypeIdInput != null ? businessTypeIdInput.text : null;
    public string GetModuleId() => moduleIdInput != null ? moduleIdInput.text : null;
    public string GetSupplierId() => supplierIdInput != null ? supplierIdInput.text : null;
    public string GetRoleId() => roleIdInput != null ? roleIdInput.text : null;
    public string GetContactId() => contactIdInput != null ? contactIdInput.text : null;

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
}
