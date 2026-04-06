using System;
using System.Collections.Generic;

[Serializable]
public class BusinessTypeDefinitionData
{
    public string id;
    public string displayName;
    public List<string> requiredModules = new List<string>();
    public int defaultStorageCapacity;
    public int defaultShelfCapacity;
    public string productType;
}

[Serializable]
public class BusinessModuleDefinitionData
{
    public string id;
    public string displayName;
    public int installCost;
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
    public string roleId;
}

[Serializable]
public class MarkupRuleDefinitionData
{
    public int minMarkup;
    public int maxMarkup;
    public int buyChance;
    public int buyMin;
    public int buyMax;
}

[Serializable]
public class CustomerBehaviorDefinitionData
{
    public string businessTypeId;
    public int arrivalRatePerHour;
    public List<MarkupRuleDefinitionData> markupRules = new List<MarkupRuleDefinitionData>();
}

[Serializable]
public class BusinessTypeDatabaseData
{
    public List<BusinessTypeDefinitionData> businessTypes = new List<BusinessTypeDefinitionData>();
}

[Serializable]
public class BusinessModuleDatabaseData
{
    public List<BusinessModuleDefinitionData> modules = new List<BusinessModuleDefinitionData>();
}

[Serializable]
public class SupplierDatabaseData
{
    public List<SupplierDefinitionData> suppliers = new List<SupplierDefinitionData>();
}

[Serializable]
public class StaffRoleDatabaseData
{
    public List<StaffRoleDefinitionData> roles = new List<StaffRoleDefinitionData>();
}

[Serializable]
public class StaffContactDatabaseData
{
    public List<StaffContactDefinitionData> contacts = new List<StaffContactDefinitionData>();
}

[Serializable]
public class CustomerBehaviorDatabaseData
{
    public List<CustomerBehaviorDefinitionData> behaviors = new List<CustomerBehaviorDefinitionData>();
}
