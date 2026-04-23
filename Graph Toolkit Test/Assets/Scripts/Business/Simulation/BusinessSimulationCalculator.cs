using Prototype.Business.Data;
using UnityEngine;

namespace Prototype.Business.Simulation
{
	public static class BusinessSimulationCalculator
	{
		private const float s_secondsPerDayDefault = 60f;

		public static void SimulateTick(
			BusinessSimulationState state,
			BusinessDefinitionsRepository definitions,
			float deltaSeconds,
			float secondsPerGameDay)
		{
			if (state == null || definitions == null || deltaSeconds <= 0f)
			{
				return;
			}

			float safeDaySeconds = secondsPerGameDay > 0f ? secondsPerGameDay : s_secondsPerDayDefault;

			state.ResetTick();

			bool hasStorageItem = !string.IsNullOrWhiteSpace(state.storageItemId);
			bool hasCashDeskItem = !string.IsNullOrWhiteSpace(state.cashDeskItemId);
			bool hasShelfItem = !string.IsNullOrWhiteSpace(state.shelfItemId);

			var tickIncome = 0f;
			var tickExpenses = 0f;

			if (state.isOpen
			    && hasStorageItem
			    && hasCashDeskItem
			    && hasShelfItem
			    && !string.IsNullOrWhiteSpace(state.hiredCashierContactId)
			    && !string.IsNullOrWhiteSpace(state.hiredMerchContactId)
			    && state.storageStock > 0f)
			{
				float demand = CalculateDemand(state, definitions, deltaSeconds, safeDaySeconds);
				state.lastDemand = demand;

				float sold = Mathf.Min(demand, state.storageStock);
				if (sold > 0f)
				{
					state.storageStock -= sold;
					state.lastSold = sold;

					float sellPrice = Mathf.Max(0f, state.markupPercent);
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

			state.shelfStock = Mathf.Max(0f, state.shelfStock);
		}

		private static float CalculateDemand(
			BusinessSimulationState state,
			BusinessDefinitionsRepository definitions,
			float deltaSeconds,
			float secondsPerGameDay)
		{
			int dailyDemand = definitions.GetDailyDemandForPrice(state.businessTypeId, state.markupPercent);
			if (dailyDemand <= 0 || secondsPerGameDay <= 0f)
			{
				return 0f;
			}

			return dailyDemand / secondsPerGameDay * deltaSeconds;
		}
	}
}
