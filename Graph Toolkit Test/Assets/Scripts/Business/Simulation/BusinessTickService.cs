using Prototype.Business.Data;
using Prototype.Business.Runtime;
using Sample.Runtime.GameData;
using UnityEngine;

namespace Prototype.Business.Simulation
{
	public sealed class BusinessTickService
	{
		private readonly BusinessCalculationService m_calculation;
		private readonly BusinessDefinitionsRepository m_definitions;

		public BusinessTickService(BusinessDefinitionsRepository definitions, GameDataRepository gameData)
		{
			m_definitions = definitions;
			m_calculation = new BusinessCalculationService(definitions, gameData);
		}

		public void RunDay(BusinessInstanceSnapshot instance)
		{
			if (instance == null)
			{
				return;
			}

			int storageCapacity = m_calculation.GetStorageCapacity(instance);
			int shelfCapacity = m_calculation.GetShelfCapacity(instance);
			int target = Mathf.Max(0, instance.autoDeliveryPerDay);
			int maxByLogist = m_calculation.GetDeliveryPerDay(instance);
			int orderedByLogist = Mathf.Min(target, maxByLogist);
			int dailyDemand = m_definitions != null
				? Mathf.Max(0, m_definitions.GetDailyDemandForPrice(instance.businessTypeId, instance.markupPercent))
				: 0;

			int storageStock = Mathf.Max(0, instance.storageStock);
			int shelfStock = Mathf.Max(0, instance.shelfStock);

			int addedToStorage = 0;
			if (orderedByLogist > 0 && storageCapacity > 0 && !string.IsNullOrWhiteSpace(instance.storageItemId))
			{
				int freeStorage = Mathf.Max(0, storageCapacity - storageStock);
				addedToStorage = Mathf.Min(orderedByLogist, freeStorage);
				storageStock += addedToStorage;
			}

			if (shelfCapacity > 0 && !string.IsNullOrWhiteSpace(instance.shelfItemId))
			{
				StaffContactDefinitionData merch = m_definitions?.GetStaffContact(instance.hiredMerchContactId);
				int throughput = merch != null ? Mathf.Max(0, merch.throughputPerHour) * 24 : 0;
				int shelfFree = Mathf.Max(0, shelfCapacity - shelfStock);
				int toShelf = merch != null && throughput > 0
					? Mathf.Min(storageStock, Mathf.Min(shelfFree, throughput))
					: 0;
				shelfStock += toShelf;
				storageStock -= toShelf;
			}

			int sold = 0;
			if (instance.isOpen && dailyDemand > 0 && !string.IsNullOrWhiteSpace(instance.hiredCashierContactId))
			{
				sold = Mathf.Min(shelfStock, dailyDemand);
				shelfStock -= sold;
			}

			int revenue = m_calculation.GetRevenueForSoldAmount(instance, sold);
			int expenses = m_calculation.GetExpensesPerDay(instance);
			SupplierDefinitionData supplier = m_definitions?.GetSupplier(instance.hiredLogistContactId);
			if (supplier != null)
			{
				int unitBuyPrice = Mathf.Max(0, supplier.unitBuyPrice);
				int baseDeliveryCost = m_calculation.GetOrderedDeliveryPerDay(instance) * unitBuyPrice;
				int actualDeliveryCost = orderedByLogist * unitBuyPrice;
				expenses += actualDeliveryCost - baseDeliveryCost;
			}
			int profit = revenue - expenses;

			instance.storageStock = Mathf.Max(0, storageStock);
			instance.shelfStock = Mathf.Max(0, shelfStock);
			instance.lastDayRevenue = revenue;
			instance.lastDayExpenses = expenses;
			instance.lastDayProfit = profit;
			instance.totalRevenue += revenue;
			instance.totalExpenses += expenses;
			instance.totalProfit += profit;
		}
	}
}
