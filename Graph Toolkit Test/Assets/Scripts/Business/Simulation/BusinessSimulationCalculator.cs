using Prototype.Business.Data;
using UnityEngine;

namespace Prototype.Business.Simulation
{
	public static class BusinessSimulationCalculator
	{
		private const float s_secondsPerHour = 3600f;
		private const float s_secondsPerDay = 86400f;
		private const string s_moduleStorage = "storage";
		private const string s_moduleShelves = "shelves";
		private const string s_moduleCashRegister = "cash_register";

		public static void SimulateTick(
			BusinessSimulationState state,
			BusinessDefinitionsRepository definitions,
			float deltaSeconds)
		{
			if (state == null || definitions == null || deltaSeconds <= 0f)
			{
				return;
			}

			state.ResetTick();

			StaffContactDefinitionData cashierContact = definitions.GetStaffContact(state.hiredCashierContactId);
			StaffContactDefinitionData merchContact = definitions.GetStaffContact(state.hiredMerchContactId);
			SupplierDefinitionData supplier = definitions.GetSupplier(state.selectedSupplierId);

			var tickIncome = 0f;
			var tickExpenses = 0f;

			if (state.rentPerDay > 0)
			{
				tickExpenses += state.rentPerDay / s_secondsPerDay * deltaSeconds;
			}

			if (!string.IsNullOrWhiteSpace(state.hiredCashierContactId) && cashierContact != null)
			{
				tickExpenses += cashierContact.salaryPerDay / s_secondsPerDay * deltaSeconds;
			}

			if (!string.IsNullOrWhiteSpace(state.hiredMerchContactId) && merchContact != null)
			{
				tickExpenses += merchContact.salaryPerDay / s_secondsPerDay * deltaSeconds;
			}

			if (supplier != null && state.autoDeliveryPerDay > 0 && state.storageCapacity > 0)
			{
				float deliveryRatePerSecond = state.autoDeliveryPerDay / s_secondsPerDay;
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

			if (state.HasModule(s_moduleStorage)
			    && state.HasModule(s_moduleShelves)
			    && !string.IsNullOrWhiteSpace(state.hiredMerchContactId)
			    && merchContact != null)
			{
				float merchRatePerSecond = merchContact.throughputPerHour / s_secondsPerHour;
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
			    && state.HasModule(s_moduleCashRegister)
			    && state.HasModule(s_moduleShelves)
			    && !string.IsNullOrWhiteSpace(state.hiredCashierContactId)
			    && cashierContact != null
			    && state.shelfStock > 0f)
			{
				float demand = CalculateDemand(state, definitions, deltaSeconds);
				state.lastDemand = demand;

				float cashierRatePerSecond = cashierContact.throughputPerHour / s_secondsPerHour;
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

		private static float CalculateDemand(
			BusinessSimulationState state,
			BusinessDefinitionsRepository definitions,
			float deltaSeconds)
		{
			CustomerBehaviorDefinitionData behavior = definitions.GetCustomerBehavior(state.businessTypeId);
			if (behavior == null || behavior.markupRules == null || behavior.markupRules.Count == 0)
			{
				return 0f;
			}

			MarkupRuleDefinitionData rule = ResolveRule(behavior, state.markupPercent);
			if (rule == null || rule.buyChance <= 0)
			{
				return 0f;
			}

			float expectedArrivals = behavior.arrivalRatePerHour * deltaSeconds / s_secondsPerHour;
			int arrivals = Mathf.FloorToInt(expectedArrivals);
			float fractional = expectedArrivals - arrivals;
			if (Random.value < fractional)
			{
				arrivals += 1;
			}

			var demand = 0f;
			int minBuy = Mathf.Max(0, rule.buyMin);
			int maxBuy = Mathf.Max(minBuy, rule.buyMax);

			for (var i = 0; i < arrivals; i++)
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
			foreach (MarkupRuleDefinitionData rule in behavior.markupRules)
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
}
