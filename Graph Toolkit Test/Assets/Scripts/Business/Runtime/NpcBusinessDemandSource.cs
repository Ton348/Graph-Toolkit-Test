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
			return TryConsumeService(business, service, 1, out _, out price);
		}

		public bool TryConsumeService(BusinessInstanceSnapshot business, NPCServiceType service, int requestedAmount, out int consumedAmount, out int price)
		{
			consumedAmount = 0;
			price = 0;
			if (business == null || !business.isOpen)
			{
				return false;
			}

			switch (service)
			{
				case NPCServiceType.Food:
				case NPCServiceType.Drink:
					int request = requestedAmount > 0 ? requestedAmount : 0;
					if (request <= 0)
					{
						return false;
					}

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

					consumedAmount = business.shelfStock < request ? business.shelfStock : request;
					if (consumedAmount <= 0)
					{
						return false;
					}

					business.shelfStock -= consumedAmount;
					price = m_calculation != null ? m_calculation.GetRevenueForSoldAmount(business, consumedAmount) : 0;
					if (price > 0)
					{
						business.dayRevenue += price;
						business.profit += price;
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
