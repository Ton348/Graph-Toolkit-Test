using UnityEngine;

public static class BusinessSimulationCalculator
{
    private const float SecondsPerHour = 3600f;
    private const float SecondsPerDay = 86400f;
    private const string ModuleStorage = "storage";
    private const string ModuleShelves = "shelves";
    private const string ModuleCashRegister = "cash_register";
    private const string RoleCashier = "cashier";
    private const string RoleMerchandiser = "merchandiser";

    public static void SimulateTick(BusinessSimulationState state, BusinessDefinitionsRepository definitions, float deltaSeconds)
    {
        if (state == null || definitions == null || deltaSeconds <= 0f)
        {
            return;
        }

        state.ResetTick();

        if (!state.isRented)
        {
            return;
        }

        var cashierRole = definitions.GetStaffRole(RoleCashier);
        var merchRole = definitions.GetStaffRole(RoleMerchandiser);
        var supplier = definitions.GetSupplier(state.selectedSupplierId);

        float tickIncome = 0f;
        float tickExpenses = 0f;

        if (state.rentPerDay > 0)
        {
            tickExpenses += state.rentPerDay / SecondsPerDay * deltaSeconds;
        }

        if (!string.IsNullOrWhiteSpace(state.hiredCashierContactId) && cashierRole != null)
        {
            tickExpenses += cashierRole.salaryPerDay / SecondsPerDay * deltaSeconds;
        }

        if (!string.IsNullOrWhiteSpace(state.hiredMerchContactId) && merchRole != null)
        {
            tickExpenses += merchRole.salaryPerDay / SecondsPerDay * deltaSeconds;
        }

        if (supplier != null && state.autoDeliveryPerDay > 0 && state.storageCapacity > 0)
        {
            float deliveryRatePerSecond = state.autoDeliveryPerDay / SecondsPerDay;
            float desired = deliveryRatePerSecond * deltaSeconds;
            float storageSpace = Mathf.Max(0f, state.storageCapacity - state.storageStock);
            float delivered = Mathf.Min(desired, storageSpace);
            if (delivered > 0f)
            {
                state.storageStock += delivered;
                state.lastDelivered = delivered;
                tickExpenses += delivered * supplier.unitBuyPrice;
            }
        }

        if (state.HasModule(ModuleStorage)
            && state.HasModule(ModuleShelves)
            && !string.IsNullOrWhiteSpace(state.hiredMerchContactId)
            && merchRole != null)
        {
            float merchRatePerSecond = merchRole.throughputPerHour / SecondsPerHour;
            float desired = merchRatePerSecond * deltaSeconds;
            float shelfSpace = Mathf.Max(0f, state.shelfCapacity - state.shelfStock);
            float moved = Mathf.Min(desired, Mathf.Min(state.storageStock, shelfSpace));
            if (moved > 0f)
            {
                state.storageStock -= moved;
                state.shelfStock += moved;
                state.lastShelved = moved;
            }
        }

        if (state.isOpen
            && state.HasModule(ModuleCashRegister)
            && state.HasModule(ModuleShelves)
            && !string.IsNullOrWhiteSpace(state.hiredCashierContactId)
            && cashierRole != null
            && state.shelfStock > 0f)
        {
            float demand = CalculateDemand(state, definitions, deltaSeconds);
            state.lastDemand = demand;

            float cashierRatePerSecond = cashierRole.throughputPerHour / SecondsPerHour;
            if (state.cashierMultiplier > 0f)
            {
                cashierRatePerSecond *= state.cashierMultiplier;
            }
            float maxSold = cashierRatePerSecond * deltaSeconds;
            float sold = Mathf.Min(demand, Mathf.Min(state.shelfStock, maxSold));
            if (sold > 0f)
            {
                state.shelfStock -= sold;
                state.lastSold = sold;

                float unitBuyPrice = supplier != null ? supplier.unitBuyPrice : 0f;
                float sellPrice = unitBuyPrice * (1f + state.markupPercent / 100f);
                tickIncome += sold * sellPrice;
            }
        }

        state.lastIncome = tickIncome;
        state.lastExpenses = tickExpenses;
        state.accumulatedIncome += tickIncome;
        state.accumulatedExpenses += tickExpenses;

        if (state.storageCapacity > 0)
        {
            state.storageStock = Mathf.Clamp(state.storageStock, 0f, state.storageCapacity);
        }
        else
        {
            state.storageStock = Mathf.Max(0f, state.storageStock);
        }

        if (state.shelfCapacity > 0)
        {
            state.shelfStock = Mathf.Clamp(state.shelfStock, 0f, state.shelfCapacity);
        }
        else
        {
            state.shelfStock = Mathf.Max(0f, state.shelfStock);
        }
    }

    private static float CalculateDemand(BusinessSimulationState state, BusinessDefinitionsRepository definitions, float deltaSeconds)
    {
        var behavior = definitions.GetCustomerBehavior(state.businessTypeId);
        if (behavior == null || behavior.markupRules == null || behavior.markupRules.Count == 0)
        {
            return 0f;
        }

        var rule = ResolveRule(behavior, state.markupPercent);
        if (rule == null || rule.buyChance <= 0)
        {
            return 0f;
        }

        float expectedArrivals = behavior.arrivalRatePerHour * deltaSeconds / SecondsPerHour;
        int arrivals = Mathf.FloorToInt(expectedArrivals);
        float fractional = expectedArrivals - arrivals;
        if (Random.value < fractional)
        {
            arrivals += 1;
        }

        float demand = 0f;
        int minBuy = Mathf.Max(0, rule.buyMin);
        int maxBuy = Mathf.Max(minBuy, rule.buyMax);

        for (int i = 0; i < arrivals; i++)
        {
            int roll = Random.Range(0, 100);
            if (roll < rule.buyChance)
            {
                int amount = Random.Range(minBuy, maxBuy + 1);
                demand += amount;
            }
        }

        return demand;
    }

    private static MarkupRuleDefinitionData ResolveRule(CustomerBehaviorDefinitionData behavior, int markupPercent)
    {
        foreach (var rule in behavior.markupRules)
        {
            if (rule == null)
            {
                continue;
            }

            if (markupPercent >= rule.minMarkup && markupPercent <= rule.maxMarkup)
            {
                return rule;
            }
        }

        return null;
    }
}
