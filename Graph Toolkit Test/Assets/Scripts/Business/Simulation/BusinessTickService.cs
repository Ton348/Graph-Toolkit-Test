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

			int storageStock = Mathf.Max(0, instance.storageStock);
			int shelfStock = Mathf.Max(0, instance.shelfStock);

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

			int dayExpenses = Mathf.Max(0, instance.dayExpenses);
			instance.storageStock = Mathf.Max(0, storageStock);
			instance.shelfStock = Mathf.Max(0, shelfStock);
			int dayRevenue = Mathf.Max(0, instance.dayRevenue);
			instance.dayExpenses = dayExpenses;
			instance.profit += dayRevenue - instance.dayExpenses;
			instance.dayRevenue = 0;
			instance.dayExpenses = 0;
			instance.dayIndex = Mathf.Max(0, instance.dayIndex) + 1;
		}
	}
}
