using System;

namespace Prototype.Business.Runtime
{
	[Serializable]
	public sealed class BusinessRuntime
	{
		public BusinessInstanceSnapshot Instance;
		public int StorageCapacity;
		public int ShelfCapacity;
		public int RentPerDay;
		public int DeliveryPerDay;
		public int ExpensesPerDay;
		public int IncomePerDay;
	}
}

