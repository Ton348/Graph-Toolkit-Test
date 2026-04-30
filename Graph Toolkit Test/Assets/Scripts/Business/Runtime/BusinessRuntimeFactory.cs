namespace Prototype.Business.Runtime
{
	public sealed class BusinessRuntimeFactory
	{
		private readonly BusinessCalculationService m_calculationService;

		public BusinessRuntimeFactory(BusinessCalculationService calculationService)
		{
			m_calculationService = calculationService;
		}

		public BusinessRuntime Build(BusinessInstanceSnapshot instance)
		{
			return new BusinessRuntime
			{
				Instance = instance,
				StorageCapacity = m_calculationService.GetStorageCapacity(instance),
				ShelfCapacity = m_calculationService.GetShelfCapacity(instance),
				RentPerDay = m_calculationService.GetRentPerDay(instance),
				DeliveryPerDay = m_calculationService.GetDeliveryPerDay(instance),
				ExpensesPerDay = m_calculationService.GetExpensesPerDay(instance),
				IncomePerDay = m_calculationService.GetIncomePerDay(instance)
			};
		}
	}
}

