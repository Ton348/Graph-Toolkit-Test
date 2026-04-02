using System.Collections.Generic;

public sealed class BusinessDefinitionsRepository
{
    private readonly Dictionary<string, BusinessTypeDefinitionData> businessTypes = new Dictionary<string, BusinessTypeDefinitionData>();
    private readonly Dictionary<string, BusinessModuleDefinitionData> modules = new Dictionary<string, BusinessModuleDefinitionData>();
    private readonly Dictionary<string, SupplierDefinitionData> suppliers = new Dictionary<string, SupplierDefinitionData>();
    private readonly Dictionary<string, StaffRoleDefinitionData> staffRoles = new Dictionary<string, StaffRoleDefinitionData>();
    private readonly Dictionary<string, CustomerBehaviorDefinitionData> behaviors = new Dictionary<string, CustomerBehaviorDefinitionData>();

    public BusinessDefinitionsRepository(
        BusinessTypeDatabaseData businessTypeDb,
        BusinessModuleDatabaseData moduleDb,
        SupplierDatabaseData supplierDb,
        StaffRoleDatabaseData staffRoleDb,
        CustomerBehaviorDatabaseData behaviorDb)
    {
        if (businessTypeDb?.businessTypes != null)
        {
            foreach (var item in businessTypeDb.businessTypes)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.id) && !businessTypes.ContainsKey(item.id))
                {
                    businessTypes[item.id] = item;
                }
            }
        }

        if (moduleDb?.modules != null)
        {
            foreach (var item in moduleDb.modules)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.id) && !modules.ContainsKey(item.id))
                {
                    modules[item.id] = item;
                }
            }
        }

        if (supplierDb?.suppliers != null)
        {
            foreach (var item in supplierDb.suppliers)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.id) && !suppliers.ContainsKey(item.id))
                {
                    suppliers[item.id] = item;
                }
            }
        }

        if (staffRoleDb?.roles != null)
        {
            foreach (var item in staffRoleDb.roles)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.id) && !staffRoles.ContainsKey(item.id))
                {
                    staffRoles[item.id] = item;
                }
            }
        }

        if (behaviorDb?.behaviors != null)
        {
            foreach (var item in behaviorDb.behaviors)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.businessTypeId) && !behaviors.ContainsKey(item.businessTypeId))
                {
                    behaviors[item.businessTypeId] = item;
                }
            }
        }
    }

    public BusinessTypeDefinitionData GetBusinessType(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        businessTypes.TryGetValue(id, out var value);
        return value;
    }

    public IReadOnlyList<string> GetRequiredModules(string businessTypeId)
    {
        var type = GetBusinessType(businessTypeId);
        return type?.requiredModules ?? new List<string>();
    }

    public bool IsModuleRequired(string businessTypeId, string moduleId)
    {
        if (string.IsNullOrWhiteSpace(moduleId)) return false;
        var type = GetBusinessType(businessTypeId);
        return type != null && type.requiredModules != null && type.requiredModules.Contains(moduleId);
    }

    public BusinessModuleDefinitionData GetModule(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        modules.TryGetValue(id, out var value);
        return value;
    }

    public SupplierDefinitionData GetSupplier(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        suppliers.TryGetValue(id, out var value);
        return value;
    }

    public StaffRoleDefinitionData GetStaffRole(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        staffRoles.TryGetValue(id, out var value);
        return value;
    }

    public CustomerBehaviorDefinitionData GetCustomerBehavior(string businessTypeId)
    {
        if (string.IsNullOrWhiteSpace(businessTypeId)) return null;
        behaviors.TryGetValue(businessTypeId, out var value);
        return value;
    }

    public bool HasBusinessType(string id) => GetBusinessType(id) != null;
    public bool HasModule(string id) => GetModule(id) != null;
    public bool HasSupplier(string id) => GetSupplier(id) != null;
    public bool HasStaffRole(string id) => GetStaffRole(id) != null;
    public bool HasCustomerBehavior(string businessTypeId) => GetCustomerBehavior(businessTypeId) != null;

    public IEnumerable<BusinessTypeDefinitionData> GetAllBusinessTypes() => businessTypes.Values;
    public IEnumerable<BusinessModuleDefinitionData> GetAllModules() => modules.Values;
    public IEnumerable<SupplierDefinitionData> GetAllSuppliers() => suppliers.Values;
    public IEnumerable<StaffRoleDefinitionData> GetAllStaffRoles() => staffRoles.Values;
    public IEnumerable<CustomerBehaviorDefinitionData> GetAllCustomerBehaviors() => behaviors.Values;
}
