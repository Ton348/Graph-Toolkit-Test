using System;
using System.Collections.Generic;
using UnityEngine;

public class BusinessSimulationService
{
    private const float s_secondsPerDay = 86400f;

    private readonly BusinessStateSyncService m_stateSync;
    private readonly Dictionary<string, BusinessRuntimeSimulationState> m_statesByLotId = new Dictionary<string, BusinessRuntimeSimulationState>();

    public float TimeScale { get; set; } = 1f;
    public bool DebugLogTicks { get; set; }

    public event Action simulationUpdated;

    public BusinessSimulationService(BusinessDefinitionsRepository definitions, BusinessStateSyncService stateSync)
    {
        this.m_stateSync = stateSync;
        if (this.m_stateSync != null)
        {
            this.m_stateSync.stateChanged += SyncFromState;
        }

        SyncFromState();
    }

    public void SimulateSeconds(float seconds)
    {
        RunTick(seconds);
    }

    public void Tick(float deltaSeconds)
    {
        RunTick(deltaSeconds);
    }

    public void RunTick(float deltaSeconds)
    {
        if (deltaSeconds <= 0f)
        {
            return;
        }

        SyncFromState();

        float scaledDelta = deltaSeconds * (TimeScale < 0f ? 0f : TimeScale);
        if (scaledDelta <= 0f)
        {
            return;
        }

        foreach (var state in m_statesByLotId.Values)
        {
            if (state == null || string.IsNullOrWhiteSpace(state.lotId) || string.IsNullOrWhiteSpace(state.businessTypeId))
            {
                continue;
            }

            float tickIncome = 0f;
            float tickExpenses = state.rentPerDay > 0 ? (state.rentPerDay / s_secondsPerDay) * scaledDelta : 0f;

            state.accumulatedIncome += tickIncome;
            state.accumulatedExpenses += tickExpenses;
            state.profit = state.accumulatedIncome - state.accumulatedExpenses;
        }

        simulationUpdated?.Invoke();
    }

    public BusinessRuntimeSimulationState GetStateByLotId(string lotId)
    {
        if (string.IsNullOrWhiteSpace(lotId))
        {
            return null;
        }

        m_statesByLotId.TryGetValue(lotId, out var state);
        return state;
    }

    public IEnumerable<BusinessRuntimeSimulationState> GetAllStates()
    {
        return m_statesByLotId.Values;
    }

    public bool TryTakeFromStorage(string lotId, float requestedAmount, out float takenAmount)
    {
        takenAmount = 0f;
        if (requestedAmount <= 0f)
        {
            return false;
        }

        var state = GetStateByLotId(lotId);
        if (state == null)
        {
            return false;
        }

        float amount = Mathf.Min(requestedAmount, Mathf.Max(0f, state.storageStock));
        if (amount <= 0f)
        {
            return false;
        }

        state.storageStock -= amount;
        state.storageStock = Mathf.Max(0f, state.storageStock);
        takenAmount = amount;
        simulationUpdated?.Invoke();
        return true;
    }

    public bool TryAddToShelves(string lotId, float requestedAmount, out float addedAmount)
    {
        addedAmount = 0f;
        if (requestedAmount <= 0f)
        {
            return false;
        }

        var state = GetStateByLotId(lotId);
        if (state == null)
        {
            return false;
        }

        float shelfSpace = state.shelfCapacity > 0 ? Mathf.Max(0f, state.shelfCapacity - state.shelfStock) : 0f;
        float amount = Mathf.Min(requestedAmount, shelfSpace);
        if (amount <= 0f)
        {
            return false;
        }

        state.shelfStock += amount;
        if (state.shelfCapacity > 0)
        {
            state.shelfStock = Mathf.Min(state.shelfStock, state.shelfCapacity);
        }
        addedAmount = amount;
        simulationUpdated?.Invoke();
        return true;
    }

    public void SetCashierMultiplier(string lotId, float multiplier)
    {
    }

    private void SyncFromState()
    {
        if (m_stateSync == null)
        {
            return;
        }

        var activeLotIds = new HashSet<string>();
        foreach (var business in m_stateSync.GetAllBusinesses())
        {
            if (business == null || string.IsNullOrWhiteSpace(business.lotId))
            {
                continue;
            }

            activeLotIds.Add(business.lotId);

            if (!m_statesByLotId.TryGetValue(business.lotId, out var state))
            {
                state = new BusinessRuntimeSimulationState
                {
                    lotId = business.lotId
                };
                m_statesByLotId[business.lotId] = state;
            }

            state.businessTypeId = business.businessTypeId;
            state.rentPerDay = business.rentPerDay;
            state.storageCapacity = business.storageCapacity;
            state.shelfCapacity = business.shelfCapacity;
            state.storageStock = business.storageStock;
            state.shelfStock = business.shelfStock;
            state.profit = state.accumulatedIncome - state.accumulatedExpenses;
        }

        var toRemove = new List<string>();
        foreach (var pair in m_statesByLotId)
        {
            if (!activeLotIds.Contains(pair.Key))
            {
                toRemove.Add(pair.Key);
            }
        }

        foreach (var lotId in toRemove)
        {
            m_statesByLotId.Remove(lotId);
        }
    }
}
