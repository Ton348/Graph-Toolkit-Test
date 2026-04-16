using System.Collections.Generic;
using System.Linq;

public class BusinessRuntimeService
{
	private readonly BusinessDefinitionsRepository m_definitions;
	private readonly BusinessStateSyncService m_stateSync;

	public BusinessRuntimeService(BusinessDefinitionsRepository definitions, BusinessStateSyncService stateSync)
	{
		m_definitions = definitions;
		m_stateSync = stateSync;
	}

	public IEnumerable<BusinessInstanceSnapshot> GetBusinesses()
	{
		return m_stateSync != null ? m_stateSync.GetAllBusinesses() : Enumerable.Empty<BusinessInstanceSnapshot>();
	}

	public BusinessInstanceSnapshot GetBusinessView(string lotId)
	{
		return m_stateSync?.GetBusinessByLotId(lotId);
	}

	public IEnumerable<SupplierDefinitionData> GetAvailableSuppliers(BusinessInstanceSnapshot business)
	{
		if (business == null || m_definitions == null)
		{
			return Enumerable.Empty<SupplierDefinitionData>();
		}

		BusinessTypeDefinitionData type = m_definitions.GetBusinessType(business.businessTypeId);
		if (type == null || string.IsNullOrWhiteSpace(type.productType))
		{
			return Enumerable.Empty<SupplierDefinitionData>();
		}

		HashSet<string> knownContacts = m_stateSync != null
			? new HashSet<string>(m_stateSync.GetKnownContacts())
			: new HashSet<string>();
		var suppliers = new List<SupplierDefinitionData>();

		foreach (SupplierDefinitionData supplier in m_definitions.GetAllSuppliers())
		{
			if (supplier == null)
			{
				continue;
			}

			if (supplier.productType == type.productType && knownContacts.Contains(supplier.id))
			{
				suppliers.Add(supplier);
			}
		}

		return suppliers;
	}

	public IEnumerable<string> GetAvailableWorkers(string roleId)
	{
		if (m_stateSync == null || m_definitions == null || string.IsNullOrWhiteSpace(roleId))
		{
			return Enumerable.Empty<string>();
		}

		var known = new HashSet<string>(m_stateSync.GetKnownContacts());
		var result = new List<string>();
		foreach (StaffContactDefinitionData contact in m_definitions.GetStaffContactsByRole(roleId))
		{
			if (contact != null && !string.IsNullOrWhiteSpace(contact.id) && known.Contains(contact.id))
			{
				result.Add(contact.id);
			}
		}

		return result;
	}

	public IEnumerable<string> GetMissingRequiredModules(BusinessInstanceSnapshot business)
	{
		if (business == null || m_definitions == null)
		{
			return Enumerable.Empty<string>();
		}

		IReadOnlyList<string> required = m_definitions.GetRequiredModules(business.businessTypeId);
		List<string> installed = business.installedModules ?? new List<string>();
		return required.Where(moduleId => !installed.Contains(moduleId));
	}
}