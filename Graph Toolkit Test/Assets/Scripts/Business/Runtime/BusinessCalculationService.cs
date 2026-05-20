using Prototype.Business.Data;
using Sample.Runtime.GameData;
using UnityEngine;

namespace Prototype.Business.Runtime
{
	public sealed class BusinessCalculationService
	{
		private readonly BusinessDefinitionsRepository m_definitions;
		private readonly GameDataRepository m_gameData;

		public BusinessCalculationService(BusinessDefinitionsRepository definitions, GameDataRepository gameData)
		{
			m_definitions = definitions;
			m_gameData = gameData;
		}

		public int GetStorageCapacity(BusinessInstanceSnapshot instance)
		{
			if (instance == null || m_definitions == null || string.IsNullOrWhiteSpace(instance.storageItemId))
			{
				return 0;
			}

			TraderItemDefinitionData item = m_definitions.GetTraderItem(instance.storageItemId);
			return item != null ? Mathf.Max(0, item.storageCapacity) : 0;
		}

		public int GetShelfCapacity(BusinessInstanceSnapshot instance)
		{
			if (instance == null || m_definitions == null || string.IsNullOrWhiteSpace(instance.shelfItemId))
			{
				return 0;
			}

			TraderItemDefinitionData item = m_definitions.GetTraderItem(instance.shelfItemId);
			return item != null ? Mathf.Max(0, item.shelfCapacity) : 0;
		}

		public int GetRentPerDay(BusinessInstanceSnapshot instance)
		{
			if (instance == null || m_gameData == null || string.IsNullOrWhiteSpace(instance.lotId))
			{
				return 0;
			}

			LotDefinitionData lot = m_gameData.GetLotById(instance.lotId);
			return lot != null ? Mathf.Max(0, lot.rentPerDay) : 0;
		}

		public int GetDeliveryPerDay(BusinessInstanceSnapshot instance)
		{
			if (instance == null || m_definitions == null || string.IsNullOrWhiteSpace(instance.hiredLogistContactId))
			{
				return 0;
			}

			StaffContactDefinitionData logist = m_definitions.GetStaffContact(instance.hiredLogistContactId);
			if (logist == null)
			{
				return 0;
			}

			return Mathf.Max(0, logist.throughputPerHour) * 24;
		}

		public int GetOrderedDeliveryPerDay(BusinessInstanceSnapshot instance)
		{
			if (instance == null)
			{
				return 0;
			}

			int target = Mathf.Max(0, instance.autoDeliveryPerDay);
			int maxByLogist = GetDeliveryPerDay(instance);
			return Mathf.Min(target, maxByLogist);
		}

		public int GetExpensesPerDay(BusinessInstanceSnapshot instance)
		{
			if (instance == null)
			{
				return 0;
			}

			int expenses = GetRentPerDay(instance);
			StaffContactDefinitionData cashier = m_definitions?.GetStaffContact(instance.hiredCashierContactId);
			StaffContactDefinitionData merch = m_definitions?.GetStaffContact(instance.hiredMerchContactId);
			StaffContactDefinitionData logist = m_definitions?.GetStaffContact(instance.hiredLogistContactId);
			expenses += cashier != null ? Mathf.Max(0, cashier.salaryPerDay) : 0;
			expenses += logist != null ? Mathf.Max(0, logist.salaryPerDay) : 0;
			if (m_definitions == null || m_definitions.RequiresMerchandiser(instance.businessTypeId))
			{
				expenses += merch != null ? Mathf.Max(0, merch.salaryPerDay) : 0;
			}

			SupplierDefinitionData supplier = m_definitions?.GetSupplier(instance.hiredLogistContactId);
			if (supplier != null)
			{
				expenses += GetOrderedDeliveryPerDay(instance) * Mathf.Max(0, supplier.unitBuyPrice);
			}

			return Mathf.Max(0, expenses);
		}

		public int GetIncomePerDay(BusinessInstanceSnapshot instance)
		{
			return 0;
		}

		public int GetProfitPerDay(BusinessInstanceSnapshot instance)
		{
			return GetIncomePerDay(instance) - GetExpensesPerDay(instance);
		}

		public int GetUnitSellPrice(BusinessInstanceSnapshot instance)
		{
			if (instance == null || m_definitions == null)
			{
				return Mathf.Max(0, instance != null ? instance.markupPercent : 0);
			}

			SupplierDefinitionData supplier = m_definitions.GetSupplier(instance.hiredLogistContactId);
			if (supplier == null)
			{
				// TODO: Use source purchase price when dedicated purchase-source config is introduced.
				return 0;
			}

			float unitBuyPrice = Mathf.Max(0f, supplier.unitBuyPrice);
			float markupFactor = 1f + Mathf.Max(0f, instance.markupPercent) / 100f;
			return Mathf.Max(0, Mathf.RoundToInt(unitBuyPrice * markupFactor));
		}

		public int GetRevenueForSoldAmount(BusinessInstanceSnapshot instance, int soldAmount)
		{
			if (soldAmount <= 0)
			{
				return 0;
			}

			int unitSellPrice = GetUnitSellPrice(instance);
			return Mathf.Max(0, soldAmount * unitSellPrice);
		}
	}
}
