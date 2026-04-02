using System;
using System.Collections.Generic;

[Serializable]
public class BusinessSimulationState
{
    public string instanceId;
    public string lotId;
    public string businessTypeId;
    public bool isRented;
    public bool isOpen;
    public int rentPerDay;
    public List<string> installedModules = new List<string>();
    public int storageCapacity;
    public int shelfCapacity;
    public float storageStock;
    public float shelfStock;
    public string selectedSupplierId;
    public int autoDeliveryPerDay;
    public int markupPercent;
    public string hiredCashierContactId;
    public string hiredMerchContactId;

    public float accumulatedIncome;
    public float accumulatedExpenses;

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
