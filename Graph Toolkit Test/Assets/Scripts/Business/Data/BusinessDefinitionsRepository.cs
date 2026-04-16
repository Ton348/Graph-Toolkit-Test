using System.Collections.Generic;

public sealed class BusinessDefinitionsRepository
{
    private readonly Dictionary<string, BusinessTypeDefinitionData> m_businessTypes = new Dictionary<string, BusinessTypeDefinitionData>();
    private readonly Dictionary<string, BusinessModuleDefinitionData> m_modules = new Dictionary<string, BusinessModuleDefinitionData>();
    private readonly Dictionary<string, SupplierDefinitionData> m_suppliers = new Dictionary<string, SupplierDefinitionData>();
    private readonly Dictionary<string, StaffRoleDefinitionData> m_staffRoles = new Dictionary<string, StaffRoleDefinitionData>();
    private readonly Dictionary<string, StaffContactDefinitionData> m_staffContacts = new Dictionary<string, StaffContactDefinitionData>();
    private readonly Dictionary<string, CustomerBehaviorDefinitionData> m_behaviors = new Dictionary<string, CustomerBehaviorDefinitionData>();

    public BusinessDefinitionsRepository(
        BusinessTypeDatabaseData businessTypeDb,
        BusinessModuleDatabaseData moduleDb,
        SupplierDatabaseData supplierDb,
        StaffRoleDatabaseData staffRoleDb,
        StaffContactDatabaseData staffContactDb,
        CustomerBehaviorDatabaseData behaviorDb)
    {
        if (businessTypeDb?.businessTypes != null)
        {
            foreach (var item in businessTypeDb.businessTypes)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.id) && !m_businessTypes.ContainsKey(item.id))
                {
                    m_businessTypes[item.id] = item;
                }
            }
        }

        if (moduleDb?.modules != null)
        {
            foreach (var item in moduleDb.modules)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.id) && !m_modules.ContainsKey(item.id))
                {
                    m_modules[item.id] = item;
                }
            }
        }

        if (supplierDb?.suppliers != null)
        {
            foreach (var item in supplierDb.suppliers)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.id) && !m_suppliers.ContainsKey(item.id))
                {
                    m_suppliers[item.id] = item;
                }
            }
        }

        if (staffRoleDb?.roles != null)
        {
            foreach (var item in staffRoleDb.roles)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.id) && !m_staffRoles.ContainsKey(item.id))
                {
                    m_staffRoles[item.id] = item;
                }
            }
        }

        if (staffContactDb?.contacts != null)
        {
            foreach (var item in staffContactDb.contacts)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.id) && !m_staffContacts.ContainsKey(item.id))
                {
                    m_staffContacts[item.id] = item;
                }
            }
        }

        if (behaviorDb?.behaviors != null)
        {
            foreach (var item in behaviorDb.behaviors)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.businessTypeId) && !m_behaviors.ContainsKey(item.businessTypeId))
                {
                    m_behaviors[item.businessTypeId] = item;
                }
            }
        }
    }

    public BusinessTypeDefinitionData GetBusinessType(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        m_businessTypes.TryGetValue(id, out var value);
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
        m_modules.TryGetValue(id, out var value);
        return value;
    }

    public SupplierDefinitionData GetSupplier(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        m_suppliers.TryGetValue(id, out var value);
        return value;
    }

    public StaffRoleDefinitionData GetStaffRole(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        m_staffRoles.TryGetValue(id, out var value);
        return value;
    }

    public CustomerBehaviorDefinitionData GetCustomerBehavior(string businessTypeId)
    {
        if (string.IsNullOrWhiteSpace(businessTypeId)) return null;
        m_behaviors.TryGetValue(businessTypeId, out var value);
        return value;
    }

    public StaffContactDefinitionData GetStaffContact(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        m_staffContacts.TryGetValue(id, out var value);
        return value;
    }

    public IEnumerable<StaffContactDefinitionData> GetAllStaffContacts() => m_staffContacts.Values;

    public IEnumerable<StaffContactDefinitionData> GetStaffContactsByRole(string roleId)
    {
        if (string.IsNullOrWhiteSpace(roleId))
        {
            yield break;
        }

        foreach (var contact in m_staffContacts.Values)
        {
            if (contact != null && contact.roleId == roleId)
            {
                yield return contact;
            }
        }
    }

    public bool HasBusinessType(string id) => GetBusinessType(id) != null;
    public bool HasModule(string id) => GetModule(id) != null;
    public bool HasSupplier(string id) => GetSupplier(id) != null;
    public bool HasStaffRole(string id) => GetStaffRole(id) != null;
    public bool HasStaffContact(string id) => GetStaffContact(id) != null;
    public bool HasCustomerBehavior(string businessTypeId) => GetCustomerBehavior(businessTypeId) != null;

    public IEnumerable<BusinessTypeDefinitionData> GetAllBusinessTypes() => m_businessTypes.Values;
    public IEnumerable<BusinessModuleDefinitionData> GetAllModules() => m_modules.Values;
    public IEnumerable<SupplierDefinitionData> GetAllSuppliers() => m_suppliers.Values;
    public IEnumerable<StaffRoleDefinitionData> GetAllStaffRoles() => m_staffRoles.Values;
    public IEnumerable<CustomerBehaviorDefinitionData> GetAllCustomerBehaviors() => m_behaviors.Values;
}
