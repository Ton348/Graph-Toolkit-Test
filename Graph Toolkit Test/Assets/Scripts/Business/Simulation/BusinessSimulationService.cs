using System;
using System.Collections.Generic;
using Prototype.Business.Data;
using Prototype.Business.Runtime;
using Sample.Runtime.GameData;
using UnityEngine;

namespace Prototype.Business.Simulation
{
	public class BusinessSimulationService
	{
		private readonly Dictionary<string, BusinessRuntimeSimulationState> m_statesByLotId = new();
		private readonly BusinessDefinitionsRepository m_definitions;
		private readonly GameDataRepository m_gameData;
		private readonly BusinessStateSyncService m_stateSync;

		public BusinessSimulationService(
			BusinessDefinitionsRepository definitions,
			GameDataRepository gameData,
			BusinessStateSyncService stateSync)
		{
			m_definitions = definitions;
			m_gameData = gameData;
			m_stateSync = stateSync;
			if (m_stateSync != null)
			{
				m_stateSync.stateChanged += SyncFromState;
			}

			SyncFromState();
		}

		public float TimeScale { get; set; } = 1f;
		public float SecondsPerGameDay { get; set; } = 60f;
		public bool DebugLogTicks { get; set; }

		public event Action simulationUpdated;

		public BusinessDefinitionsRepository GetDefinitions()
		{
			return m_definitions;
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

			float scaledDelta = deltaSeconds * (TimeScale < 0f ? 0f : TimeScale);
			if (scaledDelta <= 0f)
			{
				return;
			}

			foreach (BusinessRuntimeSimulationState state in m_statesByLotId.Values)
			{
				if (state is not BusinessSimulationState simulationState ||
				    string.IsNullOrWhiteSpace(simulationState.lotId) ||
				    string.IsNullOrWhiteSpace(simulationState.businessTypeId))
				{
					continue;
				}

				BusinessSimulationCalculator.SimulateTick(simulationState, m_definitions, scaledDelta, SecondsPerGameDay);
				simulationState.profit = simulationState.accumulatedIncome - simulationState.accumulatedExpenses;
			}

			simulationUpdated?.Invoke();
		}

		public BusinessRuntimeSimulationState GetStateByLotId(string lotId)
		{
			if (string.IsNullOrWhiteSpace(lotId))
			{
				return null;
			}

			m_statesByLotId.TryGetValue(lotId, out BusinessRuntimeSimulationState state);
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

			BusinessRuntimeSimulationState state = GetStateByLotId(lotId);
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

			BusinessRuntimeSimulationState state = GetStateByLotId(lotId);
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
			foreach (BusinessInstanceSnapshot business in m_stateSync.GetAllBusinesses())
			{
				if (business == null || string.IsNullOrWhiteSpace(business.lotId))
				{
					continue;
				}

				activeLotIds.Add(business.lotId);

				if (!m_statesByLotId.TryGetValue(business.lotId, out BusinessRuntimeSimulationState state))
				{
					state = new BusinessSimulationState
					{
						lotId = business.lotId
					};
					m_statesByLotId[business.lotId] = state;
				}

				if (state is BusinessSimulationState typedState)
				{
					BusinessRuntimeStats stats =
						BusinessRuntimeStatsCalculator.Calculate(business, m_gameData, m_definitions);
					TraderItemDefinitionData cashDeskItem =
						!string.IsNullOrWhiteSpace(business.cashDeskItemId)
							? m_definitions?.GetTraderItem(business.cashDeskItemId)
							: null;
					typedState.instanceId = business.instanceId;
					typedState.businessTypeId = business.businessTypeId;
					typedState.rentPerDay = stats.RentPerDay;
					typedState.storageCapacity = stats.StorageCapacity;
					typedState.shelfCapacity = stats.ShelfCapacity;
					typedState.storageStock = business.storageStock;
					typedState.shelfStock = business.shelfStock;
					typedState.storageItemId = business.storageItemId;
					typedState.cashDeskItemId = business.cashDeskItemId;
					typedState.shelfItemId = business.shelfItemId;
					typedState.cashCapacity = cashDeskItem != null ? Mathf.Max(0, cashDeskItem.cashCapacity) : 0;
					typedState.baseProfit = Mathf.Max(0, business.totalProfit);
					if (typedState.cashCapacity > 0 && typedState.baseProfit > typedState.cashCapacity)
					{
						typedState.baseProfit = typedState.cashCapacity;
					}
					typedState.selectedSupplierId = business.selectedSupplierId;
					typedState.autoDeliveryPerDay = business.autoDeliveryPerDay;
					typedState.markupPercent = business.markupPercent;
					typedState.hiredCashierContactId = business.hiredCashierContactId;
					typedState.hiredMerchContactId = business.hiredMerchContactId;
					typedState.isOpen = business.isOpen;
					typedState.accumulatedIncome = 0f;
					typedState.accumulatedExpenses = 0f;
					typedState.profit = typedState.accumulatedIncome - typedState.accumulatedExpenses;
				}
			}

			var toRemove = new List<string>();
			foreach (KeyValuePair<string, BusinessRuntimeSimulationState> pair in m_statesByLotId)
			{
				if (!activeLotIds.Contains(pair.Key))
				{
					toRemove.Add(pair.Key);
				}
			}

			foreach (string lotId in toRemove)
			{
				m_statesByLotId.Remove(lotId);
			}
		}
	}
}
