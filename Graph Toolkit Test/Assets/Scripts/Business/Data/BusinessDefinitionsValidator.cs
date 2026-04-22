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
			CustomerBehaviorDatabaseData behaviors,
			TraderDatabaseData traders)
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

			if (staffContacts?.contacts == null)
			{
				Debug.LogError("[BusinessDefinitions] staff contacts list is missing.");
				ok = false;
			}
			else
			{
				var ids = new HashSet<string>();

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

			if (traders?.traders == null)
			{
				Debug.LogError("[BusinessDefinitions] traders list is missing.");
				ok = false;
			}
			else
			{
				var traderIds = new HashSet<string>();
				var itemIds = new HashSet<string>();
				foreach (TraderDefinitionData trader in traders.traders)
				{
					if (trader == null || string.IsNullOrWhiteSpace(trader.id))
					{
						Debug.LogError("[BusinessDefinitions] trader has empty id.");
						ok = false;
						continue;
					}

					if (!traderIds.Add(trader.id))
					{
						Debug.LogError($"[BusinessDefinitions] duplicate trader id: {trader.id}");
						ok = false;
					}

					if (trader.items == null)
					{
						continue;
					}

					foreach (TraderItemDefinitionData item in trader.items)
					{
						if (item == null || string.IsNullOrWhiteSpace(item.id))
						{
							Debug.LogError($"[BusinessDefinitions] trader {trader.id} has item with empty id.");
							ok = false;
							continue;
						}

						if (!itemIds.Add(item.id))
						{
							Debug.LogError($"[BusinessDefinitions] duplicate trader item id: {item.id}");
							ok = false;
						}

						if (string.IsNullOrWhiteSpace(item.category))
						{
							Debug.LogError($"[BusinessDefinitions] trader item {item.id} has empty category.");
							ok = false;
						}

						if (item.price < 0)
						{
							Debug.LogError($"[BusinessDefinitions] trader item {item.id} has negative price.");
							ok = false;
						}
					}
				}
			}
			return ok;
		}
	}
}
