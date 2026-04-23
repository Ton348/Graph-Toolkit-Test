using System.Collections.Generic;

namespace Prototype.Business.Data
{
	public sealed class BusinessDefinitionsRepository
	{
		private readonly Dictionary<string, CustomerBehaviorDefinitionData> m_behaviors = new();
		private readonly Dictionary<string, BusinessTypeDefinitionData> m_businessTypes = new();
		private readonly Dictionary<string, BusinessModuleDefinitionData> m_modules = new();
		private readonly List<PriceDemandRangeDefinitionData> m_pizzeriaDemandRanges = new();
		private readonly Dictionary<string, StaffContactDefinitionData> m_staffContacts = new();
		private readonly Dictionary<string, StaffRoleDefinitionData> m_staffRoles = new();
		private readonly Dictionary<string, SupplierDefinitionData> m_suppliers = new();
		private readonly Dictionary<string, TraderDefinitionData> m_traders = new();
		private readonly Dictionary<string, TraderItemDefinitionData> m_traderItems = new();

		public BusinessDefinitionsRepository(
			BusinessTypeDatabaseData businessTypeDb,
			BusinessModuleDatabaseData moduleDb,
			SupplierDatabaseData supplierDb,
			StaffRoleDatabaseData staffRoleDb,
			StaffContactDatabaseData staffContactDb,
			CustomerBehaviorDatabaseData behaviorDb,
			PriceDemandDatabaseData pizzeriaDemandDb,
			TraderDatabaseData traderDb)
		{
			if (businessTypeDb?.businessTypes != null)
			{
				foreach (BusinessTypeDefinitionData item in businessTypeDb.businessTypes)
				{
					if (item != null && !string.IsNullOrWhiteSpace(item.id) && !m_businessTypes.ContainsKey(item.id))
					{
						m_businessTypes[item.id] = item;
					}
				}
			}

			if (moduleDb?.modules != null)
			{
				foreach (BusinessModuleDefinitionData item in moduleDb.modules)
				{
					if (item != null && !string.IsNullOrWhiteSpace(item.id) && !m_modules.ContainsKey(item.id))
					{
						m_modules[item.id] = item;
					}
				}
			}

			if (supplierDb?.suppliers != null)
			{
				foreach (SupplierDefinitionData item in supplierDb.suppliers)
				{
					if (item != null && !string.IsNullOrWhiteSpace(item.id) && !m_suppliers.ContainsKey(item.id))
					{
						m_suppliers[item.id] = item;
					}
				}
			}

			if (staffRoleDb?.roles != null)
			{
				foreach (StaffRoleDefinitionData item in staffRoleDb.roles)
				{
					if (item != null && !string.IsNullOrWhiteSpace(item.id) && !m_staffRoles.ContainsKey(item.id))
					{
						m_staffRoles[item.id] = item;
					}
				}
			}

			if (staffContactDb?.contacts != null)
			{
				foreach (StaffContactDefinitionData item in staffContactDb.contacts)
				{
					if (item != null && !string.IsNullOrWhiteSpace(item.id) && !m_staffContacts.ContainsKey(item.id))
					{
						m_staffContacts[item.id] = item;
					}
				}
			}

			if (behaviorDb?.behaviors != null)
			{
				foreach (CustomerBehaviorDefinitionData item in behaviorDb.behaviors)
				{
					if (item != null && !string.IsNullOrWhiteSpace(item.businessTypeId) &&
					    !m_behaviors.ContainsKey(item.businessTypeId))
					{
						m_behaviors[item.businessTypeId] = item;
					}
				}
			}

			if (pizzeriaDemandDb?.ranges != null)
			{
				foreach (PriceDemandRangeDefinitionData item in pizzeriaDemandDb.ranges)
				{
					if (item != null)
					{
						m_pizzeriaDemandRanges.Add(item);
					}
				}
			}

			if (traderDb?.traders != null)
			{
				foreach (TraderDefinitionData trader in traderDb.traders)
				{
					if (trader == null || string.IsNullOrWhiteSpace(trader.id) || m_traders.ContainsKey(trader.id))
					{
						continue;
					}

					m_traders[trader.id] = trader;
					if (trader.items == null)
					{
						continue;
					}

					foreach (TraderItemDefinitionData item in trader.items)
					{
						if (item == null || string.IsNullOrWhiteSpace(item.id) || m_traderItems.ContainsKey(item.id))
						{
							continue;
						}

						m_traderItems[item.id] = item;
					}
				}
			}
		}

		public BusinessTypeDefinitionData GetBusinessType(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return null;
			}

			m_businessTypes.TryGetValue(id, out BusinessTypeDefinitionData value);
			return value;
		}

		public IReadOnlyList<string> GetRequiredModules(string businessTypeId)
		{
			return new List<string>();
		}

		public bool IsModuleRequired(string businessTypeId, string moduleId)
		{
			return false;
		}

		public BusinessModuleDefinitionData GetModule(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return null;
			}

			m_modules.TryGetValue(id, out BusinessModuleDefinitionData value);
			return value;
		}

		public SupplierDefinitionData GetSupplier(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return null;
			}

			m_suppliers.TryGetValue(id, out SupplierDefinitionData value);
			return value;
		}

		public StaffRoleDefinitionData GetStaffRole(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return null;
			}

			m_staffRoles.TryGetValue(id, out StaffRoleDefinitionData value);
			return value;
		}

		public CustomerBehaviorDefinitionData GetCustomerBehavior(string businessTypeId)
		{
			if (string.IsNullOrWhiteSpace(businessTypeId))
			{
				return null;
			}

			m_behaviors.TryGetValue(businessTypeId, out CustomerBehaviorDefinitionData value);
			return value;
		}

		public int GetDailyDemandForPrice(string businessTypeId, int price)
		{
			if (string.IsNullOrWhiteSpace(businessTypeId))
			{
				return 0;
			}

			if (businessTypeId == "grocery_store")
			{
				for (int i = 0; i < m_pizzeriaDemandRanges.Count; i++)
				{
					PriceDemandRangeDefinitionData range = m_pizzeriaDemandRanges[i];
					if (range == null)
					{
						continue;
					}

					if (price >= range.minPrice && price <= range.maxPrice)
					{
						return range.dailyDemand;
					}
				}
			}

			return 0;
		}

		public StaffContactDefinitionData GetStaffContact(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return null;
			}

			m_staffContacts.TryGetValue(id, out StaffContactDefinitionData value);
			return value;
		}

		public IEnumerable<StaffContactDefinitionData> GetAllStaffContacts()
		{
			return m_staffContacts.Values;
		}

		public IEnumerable<StaffContactDefinitionData> GetStaffContactsByRole(string roleId)
		{
			foreach (StaffContactDefinitionData contact in m_staffContacts.Values)
			{
				if (contact != null)
				{
					yield return contact;
				}
			}
		}

		public bool HasBusinessType(string id)
		{
			return GetBusinessType(id) != null;
		}

		public bool HasModule(string id)
		{
			return GetModule(id) != null;
		}

		public bool HasSupplier(string id)
		{
			return GetSupplier(id) != null;
		}

		public bool HasStaffRole(string id)
		{
			return GetStaffRole(id) != null;
		}

		public bool HasStaffContact(string id)
		{
			return GetStaffContact(id) != null;
		}

		public bool HasCustomerBehavior(string businessTypeId)
		{
			return GetCustomerBehavior(businessTypeId) != null;
		}

		public IEnumerable<BusinessTypeDefinitionData> GetAllBusinessTypes()
		{
			return m_businessTypes.Values;
		}

		public IEnumerable<BusinessModuleDefinitionData> GetAllModules()
		{
			return m_modules.Values;
		}

		public IEnumerable<SupplierDefinitionData> GetAllSuppliers()
		{
			return m_suppliers.Values;
		}

		public IEnumerable<StaffRoleDefinitionData> GetAllStaffRoles()
		{
			return m_staffRoles.Values;
		}

		public IEnumerable<CustomerBehaviorDefinitionData> GetAllCustomerBehaviors()
		{
			return m_behaviors.Values;
		}

		public TraderDefinitionData GetTrader(string traderId)
		{
			if (string.IsNullOrWhiteSpace(traderId))
			{
				return null;
			}

			m_traders.TryGetValue(traderId, out TraderDefinitionData value);
			return value;
		}

		public IEnumerable<TraderDefinitionData> GetAllTraders()
		{
			return m_traders.Values;
		}

		public TraderItemDefinitionData GetTraderItem(string itemId)
		{
			if (string.IsNullOrWhiteSpace(itemId))
			{
				return null;
			}

			m_traderItems.TryGetValue(itemId, out TraderItemDefinitionData value);
			return value;
		}

		public IEnumerable<TraderItemDefinitionData> GetAllTraderItems()
		{
			return m_traderItems.Values;
		}
	}
}
