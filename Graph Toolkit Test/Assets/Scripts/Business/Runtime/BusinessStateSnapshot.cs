using System;
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
		public string selectedSupplierId;
		public int autoDeliveryPerDay;
		public int markupPercent;
		public int lastDayRevenue;
		public int lastDayExpenses;
		public int lastDayProfit;
		public int totalRevenue;
		public int totalExpenses;
		public int totalProfit;
		public string hiredCashierContactId;
		public string hiredMerchContactId;
		public string hiredLogistContactId;
	}
}
