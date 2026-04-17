using System;
using System.Collections.Generic;
using Prototype.Business.Data;
using UnityEngine;

namespace Prototype.Business.Runtime
{
	public class BusinessStateSyncService
	{
		private readonly Dictionary<string, BusinessInstanceSnapshot> m_businessesByInstanceId = new();
		private readonly Dictionary<string, BusinessInstanceSnapshot> m_businessesByLotId = new();
		private readonly BusinessDefinitionsRepository m_definitions;
		private readonly HashSet<string> m_knownContacts = new();

		public BusinessStateSyncService()
		{
		}

		public BusinessStateSyncService(BusinessDefinitionsRepository definitions)
		{
			m_definitions = definitions;
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

		public bool HasModule(string lotId, string moduleId)
		{
			BusinessInstanceSnapshot business = GetBusinessByLotId(lotId);
			return business != null && business.installedModules != null && business.installedModules.Contains(moduleId);
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
			if (business.installedModules == null)
			{
				business.installedModules = new List<string>();
			}

			if (business.markupPercent < 0 || business.markupPercent > 100)
			{
				BusinessDebugLog.Warn(
					$"[Business] Invalid markup '{business.markupPercent}' for lotId='{business.lotId}'. Clamped.");
				business.markupPercent = Mathf.Clamp(business.markupPercent, 0, 100);
			}

			if (business.storageCapacity < 0)
			{
				BusinessDebugLog.Warn($"[Business] Negative storageCapacity for lotId='{business.lotId}'. Set to 0.");
				business.storageCapacity = 0;
			}

			if (business.shelfCapacity < 0)
			{
				BusinessDebugLog.Warn($"[Business] Negative shelfCapacity for lotId='{business.lotId}'. Set to 0.");
				business.shelfCapacity = 0;
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

			if (business.storageCapacity > 0 && business.storageStock > business.storageCapacity)
			{
				BusinessDebugLog.Warn($"[Business] storageStock exceeds capacity for lotId='{business.lotId}'. Clamped.");
				business.storageStock = business.storageCapacity;
			}

			if (business.shelfCapacity > 0 && business.shelfStock > business.shelfCapacity)
			{
				BusinessDebugLog.Warn($"[Business] shelfStock exceeds capacity for lotId='{business.lotId}'. Clamped.");
				business.shelfStock = business.shelfCapacity;
			}

			var moduleSet = new HashSet<string>();
			for (int i = business.installedModules.Count - 1; i >= 0; i--)
			{
				string moduleId = business.installedModules[i];
				if (string.IsNullOrWhiteSpace(moduleId) || !moduleSet.Add(moduleId))
				{
					business.installedModules.RemoveAt(i);
					continue;
				}

				if (m_definitions != null && !m_definitions.HasModule(moduleId))
				{
					BusinessDebugLog.Warn(
						$"[Business] Unknown moduleId '{moduleId}' on lotId='{business.lotId}'. Removed.");
					business.installedModules.RemoveAt(i);
				}
			}

			if (!string.IsNullOrWhiteSpace(business.selectedSupplierId))
			{
				if (m_definitions != null && !m_definitions.HasSupplier(business.selectedSupplierId))
				{
					BusinessDebugLog.Warn(
						$"[Business] Unknown supplierId '{business.selectedSupplierId}' on lotId='{business.lotId}'. Cleared.");
					business.selectedSupplierId = null;
				}
				else if (contacts != null && !contacts.Contains(business.selectedSupplierId))
				{
					BusinessDebugLog.Warn(
						$"[Business] Supplier '{business.selectedSupplierId}' not in knownContacts for lotId='{business.lotId}'. Cleared.");
					business.selectedSupplierId = null;
				}
			}

			if (!string.IsNullOrWhiteSpace(business.hiredCashierContactId) && contacts != null &&
			    !contacts.Contains(business.hiredCashierContactId))
			{
				BusinessDebugLog.Warn(
					$"[Business] Cashier '{business.hiredCashierContactId}' not in knownContacts for lotId='{business.lotId}'. Cleared.");
				business.hiredCashierContactId = null;
			}

			if (!string.IsNullOrWhiteSpace(business.hiredMerchContactId) && contacts != null &&
			    !contacts.Contains(business.hiredMerchContactId))
			{
				BusinessDebugLog.Warn(
					$"[Business] Merchandiser '{business.hiredMerchContactId}' not in knownContacts for lotId='{business.lotId}'. Cleared.");
				business.hiredMerchContactId = null;
			}

			if (string.IsNullOrWhiteSpace(business.businessTypeId))
			{
				BusinessDebugLog.Warn($"[Business] Missing businessTypeId on lotId='{business.lotId}'.");
			}
			else if (m_definitions != null && !m_definitions.HasBusinessType(business.businessTypeId))
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
					IReadOnlyList<string> required = m_definitions.GetRequiredModules(business.businessTypeId);
					foreach (string moduleId in required)
					{
						if (!business.installedModules.Contains(moduleId))
						{
							BusinessDebugLog.Warn(
								$"[Business] Open business missing module '{moduleId}' on lotId='{business.lotId}'. Closing.");
							business.isOpen = false;
							break;
						}
					}
				}
			}
		}
	}
}