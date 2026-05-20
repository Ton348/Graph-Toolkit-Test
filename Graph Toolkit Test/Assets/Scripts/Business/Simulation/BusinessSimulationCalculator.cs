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

			state.ResetTick();
			state.lastDemand = 0f;
			state.lastSold = 0f;
			state.lastIncome = 0f;
			state.lastExpenses = 0f;

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
	}
}
