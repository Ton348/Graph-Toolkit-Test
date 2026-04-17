using System.Collections.Generic;

namespace Prototype.Business.Simulation
{
	public class BusinessSimulationState : BusinessRuntimeSimulationState
	{
		public int autoDeliveryPerDay;
		public float cashierMultiplier = 1f;
		public string hiredCashierContactId;
		public string hiredMerchContactId;
		public List<string> installedModules = new();
		public string instanceId;
		public bool isOpen;
		public float lastDelivered;
		public float lastDemand;
		public float lastExpenses;
		public float lastIncome;
		public float lastShelved;
		public float lastSold;
		public int markupPercent;
		public string selectedSupplierId;

		public bool HasModule(string moduleId)
		{
			return !string.IsNullOrWhiteSpace(moduleId) && installedModules != null && installedModules.Contains(moduleId);
		}

		public void ResetTick()
		{
			lastDelivered = 0f;
			lastShelved = 0f;
			lastDemand = 0f;
			lastSold = 0f;
			lastIncome = 0f;
			lastExpenses = 0f;
		}
	}
}
