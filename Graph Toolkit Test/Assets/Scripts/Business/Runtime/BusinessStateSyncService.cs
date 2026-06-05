using System;
using System.Collections.Generic;
using Prototype.Business.Data;
using Sample.Runtime.GameData;
using UnityEngine;

namespace Prototype.Business.Runtime
{
	public class BusinessStateSyncService
	{
		private readonly Dictionary<string, BusinessInstanceSnapshot> m_businessesByInstanceId = new();
		private readonly Dictionary<string, BusinessInstanceSnapshot> m_businessesByLotId = new();
		private readonly BusinessDefinitionsRepository m_definitions;
		private readonly GameDataRepository m_gameData;
		private readonly BusinessCalculationService m_calculation;
		private readonly HashSet<string> m_knownContacts = new();

		public BusinessStateSyncService()
		{
		}

		public BusinessStateSyncService(BusinessDefinitionsRepository definitions, GameDataRepository gameData)
		{
			m_definitions = definitions;
			m_gameData = gameData;
			m_calculation = new BusinessCalculationService(definitions, gameData);
		}

		public IReadOnlyCollection<BusinessInstanceSnapshot> Businesses => m_businessesByInstanceId.Values;
		public IReadOnlyCollection<string> KnownContacts => m_knownContacts;

		public event Action stateChanged;

		public void ApplySnapshot(ProfileSnapshot snapshot)
		{
			m_businessesByInstanceId.Clear();
			m_businessesByLotId.Clear();
			m_knownContacts.Clear();

			if (snapshot == null)
			{
				return;
			}

			if (snapshot.knownContacts != null)
			{
				foreach (string contactId in snapshot.knownContacts)
				{
					if (!string.IsNullOrWhiteSpace(contactId))
					{
						m_knownContacts.Add(contactId);
					}
				}
			}

			if (snapshot.businesses != null)
			{
				var usedLots = new HashSet<string>();
				foreach (BusinessInstanceSnapshot business in snapshot.businesses)
				{
					if (business == null)
					{
						BusinessDebugLog.Warn("[Business] Snapshot contains null business entry.");
						continue;
					}

					if (string.IsNullOrWhiteSpace(business.instanceId))
					{
						BusinessDebugLog.Warn("[Business] Snapshot business missing instanceId. Skipped.");
						continue;
					}

					if (string.IsNullOrWhiteSpace(business.lotId))
					{
						BusinessDebugLog.Warn(
							$"[Business] Snapshot business '{business.instanceId}' missing lotId. Skipped.");
						continue;
					}

					if (m_businessesByInstanceId.ContainsKey(business.instanceId))
					{
						BusinessDebugLog.Warn(
							$"[Business] Duplicate instanceId '{business.instanceId}' detected. Skipped duplicate.");
						continue;
					}

					if (!usedLots.Add(business.lotId))
					{
						BusinessDebugLog.Warn(
							$"[Business] Duplicate lotId '{business.lotId}' detected. Skipped duplicate.");
						continue;
					}

					NormalizeBusiness(business, m_knownContacts);
					m_businessesByInstanceId[business.instanceId] = business;
					m_businessesByLotId[business.lotId] = business;
				}
			}

			BusinessDebugLog.Log(
				$"[Business] Sync applied. businesses={m_businessesByInstanceId.Count} contacts={m_knownContacts.Count}");
			stateChanged?.Invoke();
		}

		public BusinessInstanceSnapshot GetBusiness(string instanceId)
		{
			if (string.IsNullOrWhiteSpace(instanceId))
			{
				return null;
			}

			m_businessesByInstanceId.TryGetValue(instanceId, out BusinessInstanceSnapshot value);
			return value;
		}

		public BusinessInstanceSnapshot GetBusinessByLotId(string lotId)
		{
			if (string.IsNullOrWhiteSpace(lotId))
			{
				return null;
			}

			m_businessesByLotId.TryGetValue(lotId, out BusinessInstanceSnapshot value);
			return value;
		}

		public bool HasBusiness(string lotId)
		{
			return GetBusinessByLotId(lotId) != null;
		}

		public bool IsBusinessOpen(string lotId)
		{
			BusinessInstanceSnapshot business = GetBusinessByLotId(lotId);
			return business != null && business.isOpen;
		}

		public IEnumerable<BusinessInstanceSnapshot> GetAllBusinesses()
		{
			return m_businessesByInstanceId.Values;
		}

		public IReadOnlyCollection<string> GetKnownContacts()
		{
			return m_knownContacts;
		}

		public bool HasKnownContact(string contactId)
		{
			return !string.IsNullOrWhiteSpace(contactId) && m_knownContacts.Contains(contactId);
		}

		private void NormalizeBusiness(BusinessInstanceSnapshot business, HashSet<string> contacts)
		{
			if (business.markupPercent < 0 || business.markupPercent > 100)
			{
				BusinessDebugLog.Warn(
					$"[Business] Invalid markup '{business.markupPercent}' for lotId='{business.lotId}'. Clamped.");
				business.markupPercent = Mathf.Clamp(business.markupPercent, 0, 100);
			}

			if (business.storageStock < 0)
			{
				BusinessDebugLog.Warn($"[Business] Negative storageStock for lotId='{business.lotId}'. Set to 0.");
				business.storageStock = 0;
			}

			if (business.shelfStock < 0)
			{
				BusinessDebugLog.Warn($"[Business] Negative shelfStock for lotId='{business.lotId}'. Set to 0.");
				business.shelfStock = 0;
			}

			int storageCapacity = m_calculation != null ? m_calculation.GetStorageCapacity(business) : 0;
			int shelfCapacity = m_calculation != null ? m_calculation.GetShelfCapacity(business) : 0;
			if (storageCapacity > 0 && business.storageStock > storageCapacity)
			{
				BusinessDebugLog.Warn($"[Business] storageStock exceeds capacity for lotId='{business.lotId}'. Clamped.");
				business.storageStock = storageCapacity;
			}

			if (shelfCapacity > 0 && business.shelfStock > shelfCapacity)
			{
				BusinessDebugLog.Warn($"[Business] shelfStock exceeds capacity for lotId='{business.lotId}'. Clamped.");
				business.shelfStock = shelfCapacity;
			}

			if (!string.IsNullOrWhiteSpace(business.hiredCashierContactId) &&
			    m_definitions != null &&
			    m_definitions.GetStaffContact(business.hiredCashierContactId) == null)
			{
				BusinessDebugLog.Warn(
					$"[Business] Unknown cashier '{business.hiredCashierContactId}' for lotId='{business.lotId}'. Cleared.");
				business.hiredCashierContactId = null;
			}

			if (!string.IsNullOrWhiteSpace(business.hiredMerchContactId) &&
			    m_definitions != null &&
			    m_definitions.GetStaffContact(business.hiredMerchContactId) == null)
			{
				BusinessDebugLog.Warn(
					$"[Business] Unknown merchandiser '{business.hiredMerchContactId}' for lotId='{business.lotId}'. Cleared.");
				business.hiredMerchContactId = null;
			}

			if (!string.IsNullOrWhiteSpace(business.hiredLogistContactId) &&
			    m_definitions != null &&
			    m_definitions.GetStaffContact(business.hiredLogistContactId) == null)
			{
				BusinessDebugLog.Warn(
					$"[Business] Unknown logistician '{business.hiredLogistContactId}' for lotId='{business.lotId}'. Cleared.");
				business.hiredLogistContactId = null;
			}

			if (!string.IsNullOrWhiteSpace(business.businessTypeId) && m_definitions != null && !m_definitions.HasBusinessType(business.businessTypeId))
			{
				BusinessDebugLog.Warn(
					$"[Business] Unknown businessTypeId '{business.businessTypeId}' on lotId='{business.lotId}'.");
			}

			if (business.isOpen)
			{
				if (string.IsNullOrWhiteSpace(business.businessTypeId))
				{
					BusinessDebugLog.Warn(
						$"[Business] Open business without businessTypeId on lotId='{business.lotId}'. Closing.");
					business.isOpen = false;
				}
				else if (m_definitions != null)
				{
					bool hasRequiredEquipment =
						!string.IsNullOrWhiteSpace(business.storageItemId) &&
						!string.IsNullOrWhiteSpace(business.cashDeskItemId) &&
						!string.IsNullOrWhiteSpace(business.shelfItemId);
					if (!hasRequiredEquipment)
					{
						BusinessDebugLog.Warn(
							$"[Business] Open business missing required equipment on lotId='{business.lotId}'. Closing.");
						business.isOpen = false;
					}
				}
			}

			if ((business.services == null || business.services.Count == 0) &&
			    m_definitions != null &&
			    !string.IsNullOrWhiteSpace(business.businessTypeId))
			{
				IReadOnlyList<Prototype.Business.NPC.Registry.NPCServiceType> services =
					m_definitions.GetServicesForBusinessType(business.businessTypeId);
				if (services != null && services.Count > 0)
				{
					business.services = new List<string>(services.Count);
					for (int i = 0; i < services.Count; i++)
					{
						business.services.Add(services[i].ToString());
					}
				}
			}
		}
	}
}
