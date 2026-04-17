using System;

namespace Prototype.Business.Simulation
{
	[Serializable]
	public class BusinessRuntimeSimulationState
	{
		public string lotId;
		public string businessTypeId;
		public int rentPerDay;

		public float storageStock;
		public float shelfStock;
		public int storageCapacity;
		public int shelfCapacity;

		public float accumulatedIncome;
		public float accumulatedExpenses;
		public float profit;
	}
}