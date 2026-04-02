using System.Collections.Generic;
using UnityEngine;

public class BusinessStateSyncService
{
    private readonly BusinessDefinitionsRepository definitions;
    private readonly Dictionary<string, BusinessInstanceSnapshot> businessesByInstanceId = new Dictionary<string, BusinessInstanceSnapshot>();
    private readonly Dictionary<string, BusinessInstanceSnapshot> businessesByLotId = new Dictionary<string, BusinessInstanceSnapshot>();
    private readonly HashSet<string> knownContacts = new HashSet<string>();

    public BusinessStateSyncService()
    {
    }

    public BusinessStateSyncService(BusinessDefinitionsRepository definitions)
    {
        this.definitions = definitions;
    }

    public IReadOnlyCollection<BusinessInstanceSnapshot> Businesses => businessesByInstanceId.Values;
    public IReadOnlyCollection<string> KnownContacts => knownContacts;

    public event System.Action StateChanged;

    public void ApplySnapshot(ProfileSnapshot snapshot)
    {
        businessesByInstanceId.Clear();
        businessesByLotId.Clear();
        knownContacts.Clear();

        if (snapshot == null)
        {
            return;
        }

        if (snapshot.KnownContacts != null)
        {
            foreach (var contactId in snapshot.KnownContacts)
            {
                if (!string.IsNullOrWhiteSpace(contactId))
                {
                    knownContacts.Add(contactId);
                }
            }
        }

        if (snapshot.Businesses != null)
        {
            var usedLots = new HashSet<string>();
            foreach (var business in snapshot.Businesses)
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
                    BusinessDebugLog.Warn($"[Business] Snapshot business '{business.instanceId}' missing lotId. Skipped.");
                    continue;
                }

                if (businessesByInstanceId.ContainsKey(business.instanceId))
                {
                    BusinessDebugLog.Warn($"[Business] Duplicate instanceId '{business.instanceId}' detected. Skipped duplicate.");
                    continue;
                }

                if (!usedLots.Add(business.lotId))
                {
                    BusinessDebugLog.Warn($"[Business] Duplicate lotId '{business.lotId}' detected. Skipped duplicate.");
                    continue;
                }

                NormalizeBusiness(business, knownContacts);
                businessesByInstanceId[business.instanceId] = business;
                businessesByLotId[business.lotId] = business;
            }
        }

        BusinessDebugLog.Log($"[Business] Sync applied. businesses={businessesByInstanceId.Count} contacts={knownContacts.Count}");
        StateChanged?.Invoke();
    }

    public BusinessInstanceSnapshot GetBusiness(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId)) return null;
        businessesByInstanceId.TryGetValue(instanceId, out var value);
        return value;
    }

    public BusinessInstanceSnapshot GetBusinessByLotId(string lotId)
    {
        if (string.IsNullOrWhiteSpace(lotId)) return null;
        businessesByLotId.TryGetValue(lotId, out var value);
        return value;
    }

    public bool HasBusiness(string lotId)
    {
        return GetBusinessByLotId(lotId) != null;
    }

    public bool IsBusinessOpen(string lotId)
    {
        var business = GetBusinessByLotId(lotId);
        return business != null && business.isOpen;
    }

    public bool HasModule(string lotId, string moduleId)
    {
        var business = GetBusinessByLotId(lotId);
        return business != null && business.installedModules != null && business.installedModules.Contains(moduleId);
    }

    public IEnumerable<BusinessInstanceSnapshot> GetAllBusinesses()
    {
        return businessesByInstanceId.Values;
    }

    public IReadOnlyCollection<string> GetKnownContacts()
    {
        return knownContacts;
    }

    public bool HasKnownContact(string contactId)
    {
        return !string.IsNullOrWhiteSpace(contactId) && knownContacts.Contains(contactId);
    }

    private void NormalizeBusiness(BusinessInstanceSnapshot business, HashSet<string> contacts)
    {
        if (business.installedModules == null)
        {
            business.installedModules = new List<string>();
        }

        if (business.markupPercent < 0 || business.markupPercent > 100)
        {
            BusinessDebugLog.Warn($"[Business] Invalid markup '{business.markupPercent}' for lotId='{business.lotId}'. Clamped.");
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

            if (definitions != null && !definitions.HasModule(moduleId))
            {
                BusinessDebugLog.Warn($"[Business] Unknown moduleId '{moduleId}' on lotId='{business.lotId}'. Removed.");
                business.installedModules.RemoveAt(i);
            }
        }

        if (!string.IsNullOrWhiteSpace(business.selectedSupplierId))
        {
            if (definitions != null && !definitions.HasSupplier(business.selectedSupplierId))
            {
                BusinessDebugLog.Warn($"[Business] Unknown supplierId '{business.selectedSupplierId}' on lotId='{business.lotId}'. Cleared.");
                business.selectedSupplierId = null;
            }
            else if (contacts != null && !contacts.Contains(business.selectedSupplierId))
            {
                BusinessDebugLog.Warn($"[Business] Supplier '{business.selectedSupplierId}' not in knownContacts for lotId='{business.lotId}'. Cleared.");
                business.selectedSupplierId = null;
            }
        }

        if (!string.IsNullOrWhiteSpace(business.hiredCashierContactId) && contacts != null && !contacts.Contains(business.hiredCashierContactId))
        {
            BusinessDebugLog.Warn($"[Business] Cashier '{business.hiredCashierContactId}' not in knownContacts for lotId='{business.lotId}'. Cleared.");
            business.hiredCashierContactId = null;
        }

        if (!string.IsNullOrWhiteSpace(business.hiredMerchContactId) && contacts != null && !contacts.Contains(business.hiredMerchContactId))
        {
            BusinessDebugLog.Warn($"[Business] Merchandiser '{business.hiredMerchContactId}' not in knownContacts for lotId='{business.lotId}'. Cleared.");
            business.hiredMerchContactId = null;
        }

        if (string.IsNullOrWhiteSpace(business.businessTypeId))
        {
            BusinessDebugLog.Warn($"[Business] Missing businessTypeId on lotId='{business.lotId}'.");
        }
        else if (definitions != null && !definitions.HasBusinessType(business.businessTypeId))
        {
            BusinessDebugLog.Warn($"[Business] Unknown businessTypeId '{business.businessTypeId}' on lotId='{business.lotId}'.");
        }

        if (business.isOpen)
        {
            if (string.IsNullOrWhiteSpace(business.businessTypeId))
            {
                BusinessDebugLog.Warn($"[Business] Open business without businessTypeId on lotId='{business.lotId}'. Closing.");
                business.isOpen = false;
            }
            else if (definitions != null)
            {
                var required = definitions.GetRequiredModules(business.businessTypeId);
                foreach (var moduleId in required)
                {
                    if (!business.installedModules.Contains(moduleId))
                    {
                        BusinessDebugLog.Warn($"[Business] Open business missing module '{moduleId}' on lotId='{business.lotId}'. Closing.");
                        business.isOpen = false;
                        break;
                    }
                }
            }
        }
    }
}
