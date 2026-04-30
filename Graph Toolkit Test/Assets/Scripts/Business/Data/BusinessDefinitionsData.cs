using System;
using System.Collections.Generic;

namespace Prototype.Business.Data
{
	[Serializable]
	public class BusinessInstanceTemplateData
	{
		public int storageStock;
		public int shelfStock;
		public int markupPercent;
		public string storageItemId;
		public string cashDeskItemId;
		public string shelfItemId;
		public string hiredCashierContactId;
		public string hiredMerchContactId;
		public string hiredLogistContactId;
		public int lastDayRevenue;
		public int lastDayExpenses;
		public int lastDayProfit;
		public int totalRevenue;
		public int totalExpenses;
		public int totalProfit;
	}

	[Serializable]
	public class BusinessTypeDefinitionData
	{
		public string id;
		public string displayName;
		public string productType;
		public BusinessInstanceTemplateData instanceTemplate;
	}

	[Serializable]
	public class SupplierDefinitionData
	{
		public string id;
		public string displayName;
		public string productType;
		public int unitBuyPrice;
		public int minDeliveryAmount;
		public int maxDeliveryAmount;
	}

	[Serializable]
	public class SupplierConfigData
	{
		public string productType;
		public int unitBuyPrice;
		public int minDeliveryAmount;
		public int maxDeliveryAmount;
	}

	[Serializable]
	public class StaffRoleDefinitionData
	{
		public string id;
		public string displayName;
		public int salaryPerDay;
		public int throughputPerHour;
	}

	[Serializable]
	public class StaffContactDefinitionData
	{
		public string id;
		public string displayName;
		public int salaryPerDay;
		public int throughputPerHour;
	}

	[Serializable]
	public class BusinessPersonDefinitionData
	{
		public string contactId;
		public string displayName;
		public int salaryPerDay;
		public int throughputPerHour;
		public SupplierConfigData supplierConfig;
	}

	[Serializable]
	public class TraderItemDefinitionData
	{
		public string id;
		public string category;
		public string name;
		public string description;
		public int price;
		public int storageCapacity;
		public int cashCapacity;
		public int shelfCapacity;
	}

	[Serializable]
	public class TraderDefinitionData
	{
		public string id;
		public string name;
		public List<string> itemIds = new();
	}

	[Serializable]
	public class BusinessTypeDatabaseData
	{
		public List<BusinessTypeDefinitionData> businessTypes = new();
	}


	[Serializable]
	public class SupplierDatabaseData
	{
		public List<SupplierDefinitionData> suppliers = new();
	}

	[Serializable]
	public class StaffRoleDatabaseData
	{
		public List<StaffRoleDefinitionData> roles = new();
	}

	[Serializable]
	public class StaffContactDatabaseData
	{
		public List<StaffContactDefinitionData> contacts = new();
	}

	[Serializable]
	public class PriceDemandRangeDefinitionData
	{
		public int minPrice;
		public int maxPrice;
		public int dailyDemand;
	}

	[Serializable]
	public class PriceDemandDatabaseData
	{
		public List<PriceDemandRangeDefinitionData> ranges = new();
	}

	[Serializable]
	public class TraderDatabaseData
	{
		public List<TraderDefinitionData> traders = new();
	}

	[Serializable]
	public class TraderItemDatabaseData
	{
		public List<TraderItemDefinitionData> items = new();
	}

	[Serializable]
	public class BusinessPeopleDatabaseData
	{
		public List<BusinessPersonDefinitionData> people = new();
	}
}
