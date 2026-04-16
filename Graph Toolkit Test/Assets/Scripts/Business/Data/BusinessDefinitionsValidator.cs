using System.Collections.Generic;
using UnityEngine;

namespace Prototype.Business.Data
{
	public static class BusinessDefinitionsValidator
	{
		public static bool Validate(
			BusinessTypeDatabaseData businessTypes,
			BusinessModuleDatabaseData modules,
			SupplierDatabaseData suppliers,
			StaffRoleDatabaseData staffRoles,
			StaffContactDatabaseData staffContacts,
			CustomerBehaviorDatabaseData behaviors)
		{
			var ok = true;
			var moduleIds = new HashSet<string>();
			var businessTypeIds = new HashSet<string>();

			if (modules?.modules == null)
			{
				Debug.LogError("[BusinessDefinitions] modules list is missing.");
				ok = false;
			}
			else
			{
				foreach (BusinessModuleDefinitionData module in modules.modules)
				{
					if (module == null || string.IsNullOrWhiteSpace(module.id))
					{
						Debug.LogError("[BusinessDefinitions] module has empty id.");
						ok = false;
						continue;
					}

					if (!moduleIds.Add(module.id))
					{
						Debug.LogError($"[BusinessDefinitions] duplicate module id: {module.id}");
						ok = false;
					}

					if (module.installCost < 0)
					{
						Debug.LogError($"[BusinessDefinitions] module {module.id} has negative installCost.");
						ok = false;
					}
				}
			}

			if (businessTypes?.businessTypes == null)
			{
				Debug.LogError("[BusinessDefinitions] business types list is missing.");
				ok = false;
			}
			else
			{
				foreach (BusinessTypeDefinitionData type in businessTypes.businessTypes)
				{
					if (type == null || string.IsNullOrWhiteSpace(type.id))
					{
						Debug.LogError("[BusinessDefinitions] business type has empty id.");
						ok = false;
						continue;
					}

					if (!businessTypeIds.Add(type.id))
					{
						Debug.LogError($"[BusinessDefinitions] duplicate business type id: {type.id}");
						ok = false;
					}

					if (type.requiredModules == null || type.requiredModules.Count == 0)
					{
						Debug.LogError($"[BusinessDefinitions] business type {type.id} has empty requiredModules.");
						ok = false;
					}
					else
					{
						foreach (string moduleId in type.requiredModules)
						{
							if (string.IsNullOrWhiteSpace(moduleId))
							{
								Debug.LogError(
									$"[BusinessDefinitions] business type {type.id} has empty module reference.");
								ok = false;
								continue;
							}

							if (!moduleIds.Contains(moduleId))
							{
								Debug.LogError(
									$"[BusinessDefinitions] business type {type.id} references missing module: {moduleId}");
								ok = false;
							}
						}
					}

					if (type.defaultShelfCapacity < 0 || type.defaultStorageCapacity < 0)
					{
						Debug.LogError($"[BusinessDefinitions] business type {type.id} has negative capacity values.");
						ok = false;
					}
				}
			}

			if (suppliers?.suppliers == null)
			{
				Debug.LogError("[BusinessDefinitions] suppliers list is missing.");
				ok = false;
			}
			else
			{
				var ids = new HashSet<string>();
				foreach (SupplierDefinitionData supplier in suppliers.suppliers)
				{
					if (supplier == null || string.IsNullOrWhiteSpace(supplier.id))
					{
						Debug.LogError("[BusinessDefinitions] supplier has empty id.");
						ok = false;
						continue;
					}

					if (!ids.Add(supplier.id))
					{
						Debug.LogError($"[BusinessDefinitions] duplicate supplier id: {supplier.id}");
						ok = false;
					}

					if (supplier.unitBuyPrice < 0 || supplier.minDeliveryAmount < 0 || supplier.maxDeliveryAmount < 0)
					{
						Debug.LogError($"[BusinessDefinitions] supplier {supplier.id} has negative values.");
						ok = false;
					}

					if (supplier.minDeliveryAmount > supplier.maxDeliveryAmount)
					{
						Debug.LogError(
							$"[BusinessDefinitions] supplier {supplier.id} minDeliveryAmount > maxDeliveryAmount.");
						ok = false;
					}
				}
			}

			if (staffRoles?.roles == null)
			{
				Debug.LogError("[BusinessDefinitions] staff roles list is missing.");
				ok = false;
			}
			else
			{
				var ids = new HashSet<string>();
				foreach (StaffRoleDefinitionData role in staffRoles.roles)
				{
					if (role == null || string.IsNullOrWhiteSpace(role.id))
					{
						Debug.LogError("[BusinessDefinitions] staff role has empty id.");
						ok = false;
						continue;
					}

					if (!ids.Add(role.id))
					{
						Debug.LogError($"[BusinessDefinitions] duplicate staff role id: {role.id}");
						ok = false;
					}

					if (role.salaryPerDay < 0 || role.throughputPerHour < 0)
					{
						Debug.LogError($"[BusinessDefinitions] staff role {role.id} has negative values.");
						ok = false;
					}
				}
			}

			if (staffContacts?.contacts == null)
			{
				Debug.LogError("[BusinessDefinitions] staff contacts list is missing.");
				ok = false;
			}
			else
			{
				var ids = new HashSet<string>();
				var roleIds = new HashSet<string>();
				if (staffRoles?.roles != null)
				{
					foreach (StaffRoleDefinitionData role in staffRoles.roles)
					{
						if (role != null && !string.IsNullOrWhiteSpace(role.id))
						{
							roleIds.Add(role.id);
						}
					}
				}

				foreach (StaffContactDefinitionData contact in staffContacts.contacts)
				{
					if (contact == null || string.IsNullOrWhiteSpace(contact.id))
					{
						Debug.LogError("[BusinessDefinitions] staff contact has empty id.");
						ok = false;
						continue;
					}

					if (!ids.Add(contact.id))
					{
						Debug.LogError($"[BusinessDefinitions] duplicate staff contact id: {contact.id}");
						ok = false;
					}

					if (contact.salaryPerDay < 0 || contact.throughputPerHour < 0)
					{
						Debug.LogError($"[BusinessDefinitions] staff contact {contact.id} has negative values.");
						ok = false;
					}

					if (string.IsNullOrWhiteSpace(contact.roleId))
					{
						Debug.LogError($"[BusinessDefinitions] staff contact {contact.id} has empty roleId.");
						ok = false;
						continue;
					}

					if (!roleIds.Contains(contact.roleId))
					{
						Debug.LogError(
							$"[BusinessDefinitions] staff contact {contact.id} references unknown roleId: {contact.roleId}");
						ok = false;
					}
				}
			}

			if (behaviors?.behaviors == null)
			{
				Debug.LogError("[BusinessDefinitions] customer behaviors list is missing.");
				ok = false;
			}
			else
			{
				var ids = new HashSet<string>();
				foreach (CustomerBehaviorDefinitionData behavior in behaviors.behaviors)
				{
					if (behavior == null || string.IsNullOrWhiteSpace(behavior.businessTypeId))
					{
						Debug.LogError("[BusinessDefinitions] customer behavior missing businessTypeId.");
						ok = false;
						continue;
					}

					if (!ids.Add(behavior.businessTypeId))
					{
						Debug.LogError(
							$"[BusinessDefinitions] duplicate customer behavior for businessTypeId: {behavior.businessTypeId}");
						ok = false;
					}

					if (!businessTypeIds.Contains(behavior.businessTypeId))
					{
						Debug.LogError(
							$"[BusinessDefinitions] customer behavior references unknown businessTypeId: {behavior.businessTypeId}");
						ok = false;
					}

					if (behavior.arrivalRatePerHour < 0)
					{
						Debug.LogError(
							$"[BusinessDefinitions] customer behavior {behavior.businessTypeId} has negative arrivalRatePerHour.");
						ok = false;
					}
				}
			}

			return ok;
		}
	}
}