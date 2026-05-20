using System.Collections.Generic;

namespace Prototype.Business.Data
{
	public sealed class BusinessDefinitionsRepository
	{
		private readonly Dictionary<string, BusinessTypeDefinitionData> m_businessTypes = new();
		private readonly Dictionary<string, StaffContactDefinitionData> m_staffContacts = new();
		private readonly Dictionary<string, StaffRoleDefinitionData> m_staffRoles = new();
		private readonly Dictionary<string, SupplierDefinitionData> m_suppliers = new();
		private readonly Dictionary<string, BusinessPersonDefinitionData> m_people = new();
		private readonly Dictionary<string, TraderDefinitionData> m_traders = new();
		private readonly Dictionary<string, TraderItemDefinitionData> m_traderItems = new();

		public BusinessDefinitionsRepository(
			BusinessTypeDatabaseData businessTypeDb,
			SupplierDatabaseData supplierDb,
			StaffRoleDatabaseData staffRoleDb,
			StaffContactDatabaseData staffContactDb,
			BusinessPeopleDatabaseData peopleDb,
			TraderDatabaseData traderDb,
			TraderItemDatabaseData traderItemDb)
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

			if (peopleDb?.people != null)
			{
				foreach (BusinessPersonDefinitionData item in peopleDb.people)
				{
					if (item != null && !string.IsNullOrWhiteSpace(item.contactId) && !m_people.ContainsKey(item.contactId))
					{
						m_people[item.contactId] = item;
					}
				}
			}

			if (traderItemDb?.items != null)
			{
				foreach (TraderItemDefinitionData item in traderItemDb.items)
				{
					if (item == null || string.IsNullOrWhiteSpace(item.id) || m_traderItems.ContainsKey(item.id))
					{
						continue;
					}

					m_traderItems[item.id] = item;
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

		public IReadOnlyList<Prototype.Business.NPC.Registry.NPCServiceType> GetServicesForBusinessType(string businessTypeId)
		{
			BusinessTypeDefinitionData type = GetBusinessType(businessTypeId);
			if (type == null)
			{
				return System.Array.Empty<Prototype.Business.NPC.Registry.NPCServiceType>();
			}

			if (type.services != null && type.services.Count > 0)
			{
				return type.services;
			}

			if (string.Equals(type.productType, "grocery", System.StringComparison.OrdinalIgnoreCase))
			{
				return new[]
				{
					Prototype.Business.NPC.Registry.NPCServiceType.Food,
					Prototype.Business.NPC.Registry.NPCServiceType.Drink
				};
			}

			return System.Array.Empty<Prototype.Business.NPC.Registry.NPCServiceType>();
		}

		public bool RequiresMerchandiser(string businessTypeId)
		{
			BusinessTypeDefinitionData type = GetBusinessType(businessTypeId);
			return type == null ||
			       (type.instanceTemplate != null &&
			        type.instanceTemplate.hiredMerchContactId != null);
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

		public IEnumerable<BusinessTypeDefinitionData> GetAllBusinessTypes()
		{
			return m_businessTypes.Values;
		}

		public IEnumerable<SupplierDefinitionData> GetAllSuppliers()
		{
			return m_suppliers.Values;
		}

		public IEnumerable<StaffRoleDefinitionData> GetAllStaffRoles()
		{
			return m_staffRoles.Values;
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

		public bool TraderSellsItem(string traderId, string itemId)
		{
			TraderDefinitionData trader = GetTrader(traderId);
			if (trader?.itemIds == null || string.IsNullOrWhiteSpace(itemId))
			{
				return false;
			}

			for (int i = 0; i < trader.itemIds.Count; i++)
			{
				if (trader.itemIds[i] == itemId)
				{
					return true;
				}
			}

			return false;
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
