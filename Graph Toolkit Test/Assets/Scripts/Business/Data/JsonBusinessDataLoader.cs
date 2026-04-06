using System.IO;
using UnityEngine;

public sealed class JsonBusinessDataLoader
{
    private readonly string rootPath;

    public JsonBusinessDataLoader(string rootPath)
    {
        this.rootPath = rootPath;
    }

    public BusinessTypeDatabaseData LoadBusinessTypes() => Load<BusinessTypeDatabaseData>("business_types.json", "business types");
    public BusinessModuleDatabaseData LoadBusinessModules() => Load<BusinessModuleDatabaseData>("business_modules.json", "business modules");
    public SupplierDatabaseData LoadSuppliers() => Load<SupplierDatabaseData>("suppliers.json", "suppliers");
    public StaffRoleDatabaseData LoadStaffRoles() => Load<StaffRoleDatabaseData>("staff_roles.json", "staff roles");
    public StaffContactDatabaseData LoadStaffContacts() => Load<StaffContactDatabaseData>("staff_contacts.json", "staff contacts");
    public CustomerBehaviorDatabaseData LoadCustomerBehaviors() => Load<CustomerBehaviorDatabaseData>("customer_behavior.json", "customer behaviors");

    private T Load<T>(string fileName, string label) where T : class
    {
        string path = Path.Combine(rootPath, fileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"[JsonBusinessDataLoader] Missing {label} file: {path}");
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogError($"[JsonBusinessDataLoader] Empty JSON for {label}: {path}");
                return null;
            }

            var data = JsonUtility.FromJson<T>(json);
            if (data == null)
            {
                Debug.LogError($"[JsonBusinessDataLoader] Failed to parse {label}: {path}");
                return null;
            }

            Debug.Log($"[JsonBusinessDataLoader] Loaded {label}");
            return data;
        }
        catch (System.SystemException ex)
        {
            Debug.LogError($"[JsonBusinessDataLoader] Error reading {label}: {ex.Message}");
            return null;
        }
    }
}
