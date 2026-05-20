using System.Collections.Generic;

namespace Prototype.Business.NPC.Workers
{
	public static class WorkerNPCRegistry
	{
		private static readonly Dictionary<string, WorkerNPCBrain> s_cashierByLotId = new();

		public static void SetCashierActive(string lotId, bool active, WorkerNPCBrain brain)
		{
			if (string.IsNullOrWhiteSpace(lotId))
			{
				return;
			}

			if (active)
			{
				s_cashierByLotId[lotId] = brain;
			}
			else
			{
				s_cashierByLotId.Remove(lotId);
			}
		}

		public static bool TryGetActiveCashier(string lotId, out WorkerNPCBrain brain)
		{
			brain = null;
			if (string.IsNullOrWhiteSpace(lotId))
			{
				return false;
			}

			if (!s_cashierByLotId.TryGetValue(lotId, out WorkerNPCBrain value) || value == null)
			{
				return false;
			}

			brain = value;
			return true;
		}
	}
}
