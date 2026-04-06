using System;
using System.Collections.Generic;

public class BusinessSimulationState : BusinessRuntimeSimulationState
{
    public string instanceId;
    public bool isRented;
    public bool isOpen;
    public List<string> installedModules = new List<string>();
    public string selectedSupplierId;
    public int autoDeliveryPerDay;
    public int markupPercent;
    public string hiredCashierContactId;
    public string hiredMerchContactId;
    public float cashierMultiplier = 1f;
    public float lastDelivered;
    public float lastShelved;
    public float lastDemand;
    public float lastSold;
    public float lastIncome;
    public float lastExpenses;

    public bool HasModule(string moduleId)
    {
        return !string.IsNullOrWhiteSpace(moduleId) && installedModules != null && installedModules.Contains(moduleId);
    }

    public void ResetTick()
    {
        lastDelivered = 0f;
        lastShelved = 0f;
        lastDemand = 0f;
        lastSold = 0f;
        lastIncome = 0f;
        lastExpenses = 0f;
    }
}
