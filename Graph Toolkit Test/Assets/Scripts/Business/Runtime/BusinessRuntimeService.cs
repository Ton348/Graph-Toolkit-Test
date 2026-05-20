using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prototype.Business.Data;
using Prototype.Business.NPC.Registry;
using Prototype.Business.Services;
using UnityEngine;

namespace Prototype.Business.Runtime
{
	public class BusinessRuntimeService
	{
		private readonly BusinessDefinitionsRepository m_definitions;
		private readonly BusinessStateSyncService m_stateSync;
		private readonly IBusinessDemandSource m_npcDemandSource;
		private readonly BusinessCalculationService m_calculation;
		private readonly IGameServer m_gameServer;
		private readonly ProfileSyncService m_profileSync;

		public BusinessRuntimeService(
			BusinessDefinitionsRepository definitions,
			BusinessStateSyncService stateSync,
			IGameServer gameServer = null,
			ProfileSyncService profileSync = null)
		{
			m_definitions = definitions;
			m_stateSync = stateSync;
			m_calculation = new BusinessCalculationService(definitions, null);
			m_npcDemandSource = new NpcBusinessDemandSource(m_calculation);
			m_gameServer = gameServer;
			m_profileSync = profileSync;
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

		public IEnumerable<string> GetAvailableWorkers(string _)
		{
			if (m_stateSync == null || m_definitions == null)
			{
				return Enumerable.Empty<string>();
			}

			var known = new HashSet<string>(m_stateSync.GetKnownContacts());
			var result = new List<string>();
			foreach (StaffContactDefinitionData contact in m_definitions.GetAllStaffContacts())
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
			return Enumerable.Empty<string>();
		}

		public bool TryConsumeService(string lotId, NPC.Registry.NPCServiceType service, out int price)
		{
			price = 0;
			if (string.IsNullOrWhiteSpace(lotId) || m_stateSync == null || m_npcDemandSource == null)
			{
				return false;
			}

			BusinessInstanceSnapshot business = m_stateSync.GetBusinessByLotId(lotId);
			if (business == null)
			{
				return false;
			}

			return m_npcDemandSource.TryConsumeService(business, service, out price);
		}

		public bool TryConsumeService(string lotId, NPC.Registry.NPCServiceType service, int requestedAmount, out int consumedAmount, out int price)
		{
			consumedAmount = 0;
			price = 0;
			if (string.IsNullOrWhiteSpace(lotId) || m_stateSync == null)
			{
				return false;
			}

			BusinessInstanceSnapshot business = m_stateSync.GetBusinessByLotId(lotId);
			if (business == null)
			{
				return false;
			}

			if (m_gameServer != null)
			{
				return false;
			}

			if (m_npcDemandSource is NpcBusinessDemandSource npcSource)
			{
				return npcSource.TryConsumeService(business, service, requestedAmount, out consumedAmount, out price);
			}

			bool ok = m_npcDemandSource != null && m_npcDemandSource.TryConsumeService(business, service, out price);
			consumedAmount = ok ? 1 : 0;
			return ok;
		}

		public async Task<(bool success, int consumedAmount, int price)> TryConsumeServiceAsync(
			string lotId,
			NPCServiceType service,
			int requestedAmount)
		{
			if (string.IsNullOrWhiteSpace(lotId) || m_stateSync == null)
			{
				return (false, 0, 0);
			}

			BusinessInstanceSnapshot business = m_stateSync.GetBusinessByLotId(lotId);
			if (business == null)
			{
				return (false, 0, 0);
			}

			if (m_gameServer != null)
			{
				int shelfBefore = business.shelfStock;
				int dayRevenueBefore = business.dayRevenue;
				ServerActionResult result =
					await m_gameServer.TryConsumeNpcServiceAsync(lotId, service.ToString(), requestedAmount);
				if (result != null && result.Success && result.ProfileSnapshot != null)
				{
					m_profileSync?.ApplySnapshot(result.ProfileSnapshot);
					BusinessInstanceSnapshot updated = m_stateSync.GetBusinessByLotId(lotId);
					if (updated != null)
					{
						int consumedAmount = Mathf.Max(0, shelfBefore - updated.shelfStock);
						int price = Mathf.Max(0, updated.dayRevenue - dayRevenueBefore);
						bool ok = consumedAmount > 0 || service == NPCServiceType.Toilet || service == NPCServiceType.Sleep ||
						          service == NPCServiceType.Work || service == NPCServiceType.Fun;
						return (ok, consumedAmount, price);
					}
				}

				return (false, 0, 0);
			}

			if (m_npcDemandSource is NpcBusinessDemandSource npcSource)
			{
				bool ok = npcSource.TryConsumeService(business, service, requestedAmount, out int consumedAmount, out int price);
				return (ok, consumedAmount, price);
			}

			int localPrice = 0;
			bool localOk = m_npcDemandSource != null && m_npcDemandSource.TryConsumeService(business, service, out localPrice);
			return (localOk, localOk ? 1 : 0, localPrice);
		}
	}
}
