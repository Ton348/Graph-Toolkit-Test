using System;
using Prototype.Business.Time;
namespace Prototype.Business.Runtime
{
	[Serializable]
	public class BusinessInstanceSnapshot
	{
		public string instanceId;
		public string lotId;
		public string businessTypeId;
		public bool isOpen;
		public int storageStock;
		public int shelfStock;
		public string storageItemId;
		public string cashDeskItemId;
		public string shelfItemId;
		public int autoDeliveryPerDay;
		public int markupPercent;
		public int dayRevenue;
		public int dayExpenses;
		public int dayIndex;
		public int profit;
		public string hiredCashierContactId;
		public string hiredMerchContactId;
		public string hiredLogistContactId;
		public float OpenHour = 8f;
		public float CloseHour = 22f;

		public bool IsOpenNow()
		{
			return WorldTimeSystem.Instance != null && WorldTimeSystem.Instance.IsBusinessHours(OpenHour, CloseHour);
		}

		public int DayProfit => dayRevenue - dayExpenses;
	}
}
