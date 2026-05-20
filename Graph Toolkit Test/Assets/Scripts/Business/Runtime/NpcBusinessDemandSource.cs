using Prototype.Business.NPC.Registry;
using Prototype.Business.NPC.Workers;

namespace Prototype.Business.Runtime
{
	public sealed class NpcBusinessDemandSource : IBusinessDemandSource
	{
		private readonly BusinessCalculationService m_calculation;

		public NpcBusinessDemandSource(BusinessCalculationService calculation)
		{
			m_calculation = calculation;
		}

		public bool TryConsumeService(BusinessInstanceSnapshot business, NPCServiceType service, out int price)
		{
			price = 0;
			if (business == null || !business.isOpen)
			{
				return false;
			}

			switch (service)
			{
				case NPCServiceType.Food:
				case NPCServiceType.Drink:
					if (string.IsNullOrWhiteSpace(business.lotId) ||
					    !WorkerNPCRegistry.TryGetActiveCashier(business.lotId, out WorkerNPCBrain cashier) ||
					    cashier == null ||
					    !cashier.CanServeNow())
					{
						return false;
					}

					if (business.shelfStock <= 0)
					{
						return false;
					}

					business.shelfStock -= 1;
					price = m_calculation != null ? m_calculation.GetRevenueForSoldAmount(business, 1) : 0;
					if (price > 0)
					{
						business.lastDayRevenue += price;
						business.totalRevenue += price;
						business.lastDayProfit += price;
						business.totalProfit += price;
					}

					return true;

				case NPCServiceType.Toilet:
				case NPCServiceType.Sleep:
				case NPCServiceType.Work:
				case NPCServiceType.Fun:
					return true;

				default:
					return false;
			}
		}
	}
}
