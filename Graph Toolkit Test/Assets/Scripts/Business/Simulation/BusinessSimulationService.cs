using System;
using System.Collections.Generic;
using UnityEngine;

public class BusinessSimulationService
{
    private readonly BusinessDefinitionsRepository definitions;
    private readonly BusinessStateSyncService stateSync;
    private readonly Dictionary<string, BusinessSimulationState> statesByLotId = new Dictionary<string, BusinessSimulationState>();

    public float TimeScale { get; set; } = 1f;
    public bool DebugLogTicks { get; set; }

    public event Action SimulationUpdated;

    public BusinessSimulationService(BusinessDefinitionsRepository definitions, BusinessStateSyncService stateSync)
    {
        this.definitions = definitions;
        this.stateSync = stateSync;

        if (stateSync != null)
        {
            stateSync.StateChanged += SyncFromState;
        }

        SyncFromState();
    }

    public void SimulateSeconds(float seconds)
    {
        if (seconds > 0f)
        {
            BusinessDebugLog.Log($"[BusinessSim] FastForward seconds={seconds:0.##}");
        }
        Tick(seconds);
    }

    public void Tick(float deltaSeconds)
    {
        if (deltaSeconds <= 0f || definitions == null)
        {
            return;
        }

        float scaled = Mathf.Max(0f, deltaSeconds * Mathf.Max(0f, TimeScale));
        if (scaled <= 0f)
        {
            return;
        }

        foreach (var state in statesByLotId.Values)
        {
            BusinessSimulationCalculator.SimulateTick(state, definitions, scaled);

            if (DebugLogTicks)
            {
                BusinessDebugLog.Log($"[BusinessSim] lotId='{state.lotId}' demand={state.lastDemand:0.##} delivered={state.lastDelivered:0.##} shelved={state.lastShelved:0.##} sold={state.lastSold:0.##} income={state.lastIncome:0.##} expenses={state.lastExpenses:0.##}");
            }
        }

        SimulationUpdated?.Invoke();
    }

    public BusinessSimulationState GetStateByLotId(string lotId)
    {
        if (string.IsNullOrWhiteSpace(lotId))
        {
            return null;
        }

        statesByLotId.TryGetValue(lotId, out var value);
        return value;
    }

    public IEnumerable<BusinessSimulationState> GetAllStates()
    {
        return statesByLotId.Values;
    }

    public void SetCashierMultiplier(string lotId, float multiplier)
    {
        var state = GetStateByLotId(lotId);
        if (state == null)
        {
            return;
        }

        state.cashierMultiplier = Mathf.Max(0f, multiplier);
        BusinessDebugLog.Log($"[BusinessSim] Set cashier multiplier lotId='{lotId}' value={state.cashierMultiplier:0.##}");
    }

    private void SyncFromState()
    {
        if (stateSync == null)
        {
            return;
        }

        var activeLots = new HashSet<string>();
        foreach (var business in stateSync.GetAllBusinesses())
        {
            if (business == null || string.IsNullOrWhiteSpace(business.lotId))
            {
                continue;
            }

            activeLots.Add(business.lotId);

            if (statesByLotId.TryGetValue(business.lotId, out var state))
            {
                ApplySnapshot(state, business);
            }
            else
            {
                state = new BusinessSimulationState();
                ApplySnapshot(state, business);
                statesByLotId[business.lotId] = state;
            }
        }

        var toRemove = new List<string>();
        foreach (var pair in statesByLotId)
        {
            if (!activeLots.Contains(pair.Key))
            {
                toRemove.Add(pair.Key);
            }
        }

        foreach (var key in toRemove)
        {
            statesByLotId.Remove(key);
        }

        BusinessDebugLog.Log($"[BusinessSim] Sync from state. businesses={statesByLotId.Count}");
        SimulationUpdated?.Invoke();
    }

    private static void ApplySnapshot(BusinessSimulationState state, BusinessInstanceSnapshot snapshot)
    {
        state.instanceId = snapshot.instanceId;
        state.lotId = snapshot.lotId;
        state.businessTypeId = snapshot.businessTypeId;
        state.isRented = snapshot.isRented;
        state.isOpen = snapshot.isOpen;
        state.rentPerDay = snapshot.rentPerDay;
        state.installedModules = snapshot.installedModules != null ? new List<string>(snapshot.installedModules) : new List<string>();
        state.storageCapacity = snapshot.storageCapacity;
        state.shelfCapacity = snapshot.shelfCapacity;
        state.selectedSupplierId = snapshot.selectedSupplierId;
        state.autoDeliveryPerDay = snapshot.autoDeliveryPerDay;
        state.markupPercent = snapshot.markupPercent;
        state.hiredCashierContactId = snapshot.hiredCashierContactId;
        state.hiredMerchContactId = snapshot.hiredMerchContactId;

        state.storageStock = snapshot.storageStock;
        state.shelfStock = snapshot.shelfStock;

        if (state.storageCapacity > 0)
        {
            state.storageStock = Mathf.Min(state.storageStock, state.storageCapacity);
        }
        if (state.shelfCapacity > 0)
        {
            state.shelfStock = Mathf.Min(state.shelfStock, state.shelfCapacity);
        }
    }
}
