using System.Collections.Generic;
using System.Linq;

public class BusinessRuntimeService
{
    private readonly BusinessDefinitionsRepository definitions;
    private readonly BusinessStateSyncService stateSync;

    public BusinessRuntimeService(BusinessDefinitionsRepository definitions, BusinessStateSyncService stateSync)
    {
        this.definitions = definitions;
        this.stateSync = stateSync;
    }

    public IEnumerable<BusinessInstanceSnapshot> GetBusinesses()
    {
        return stateSync != null ? stateSync.GetAllBusinesses() : Enumerable.Empty<BusinessInstanceSnapshot>();
    }

    public BusinessInstanceSnapshot GetBusinessView(string lotId)
    {
        return stateSync?.GetBusinessByLotId(lotId);
    }

    public IEnumerable<SupplierDefinitionData> GetAvailableSuppliers(BusinessInstanceSnapshot business)
    {
        if (business == null || definitions == null)
        {
            return Enumerable.Empty<SupplierDefinitionData>();
        }

        var type = definitions.GetBusinessType(business.businessTypeId);
        if (type == null || string.IsNullOrWhiteSpace(type.productType))
        {
            return Enumerable.Empty<SupplierDefinitionData>();
        }

        var knownContacts = stateSync != null ? new HashSet<string>(stateSync.GetKnownContacts()) : new HashSet<string>();
        var suppliers = new List<SupplierDefinitionData>();

        foreach (var supplier in definitions.GetAllSuppliers())
        {
            if (supplier == null) continue;
            if (supplier.productType == type.productType && knownContacts.Contains(supplier.id))
            {
                suppliers.Add(supplier);
            }
        }

        return suppliers;
    }

    public IEnumerable<string> GetAvailableWorkers(string roleId)
    {
        if (stateSync == null)
        {
            return Enumerable.Empty<string>();
        }

        return stateSync.GetKnownContacts();
    }

    public IEnumerable<string> GetMissingRequiredModules(BusinessInstanceSnapshot business)
    {
        if (business == null || definitions == null)
        {
            return Enumerable.Empty<string>();
        }

        var required = definitions.GetRequiredModules(business.businessTypeId);
        var installed = business.installedModules ?? new List<string>();
        return required.Where(moduleId => !installed.Contains(moduleId));
    }

}
