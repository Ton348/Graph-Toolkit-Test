function normalizeBusinessInstance(business) {
  function normalizeOptionalId(value) {
    if (typeof value !== 'string') {
      return null;
    }

    const trimmed = value.trim();
    return trimmed.length > 0 ? trimmed : null;
  }

  const normalized = {
    instanceId: typeof business?.instanceId === 'string' ? business.instanceId : '',
    lotId: typeof business?.lotId === 'string' ? business.lotId : '',
    businessTypeId: typeof business?.businessTypeId === 'string' ? business.businessTypeId : '',
    isOpen: Boolean(business?.isOpen),
    storageStock: Number.isFinite(business?.storageStock) ? business.storageStock : 0,
    shelfStock: Number.isFinite(business?.shelfStock) ? business.shelfStock : 0,
    autoDeliveryPerDay: Number.isFinite(business?.autoDeliveryPerDay) ? business.autoDeliveryPerDay : 0,
    markupPercent: Number.isFinite(business?.markupPercent) ? business.markupPercent : 0,
    lastDayRevenue: Number.isFinite(business?.lastDayRevenue) ? business.lastDayRevenue : 0,
    lastDayExpenses: Number.isFinite(business?.lastDayExpenses) ? business.lastDayExpenses : 0,
    lastDayProfit: Number.isFinite(business?.lastDayProfit) ? business.lastDayProfit : 0,
    totalRevenue: Number.isFinite(business?.totalRevenue) ? business.totalRevenue : 0,
    totalExpenses: Number.isFinite(business?.totalExpenses) ? business.totalExpenses : 0,
    totalProfit: Number.isFinite(business?.totalProfit) ? business.totalProfit : 0
  };

  if (normalized.storageStock < 0) normalized.storageStock = 0;
  if (normalized.shelfStock < 0) normalized.shelfStock = 0;
  if (normalized.autoDeliveryPerDay < 0) normalized.autoDeliveryPerDay = 0;
  if (normalized.markupPercent < 0) normalized.markupPercent = 0;
  if (normalized.lastDayRevenue < 0) normalized.lastDayRevenue = 0;
  if (normalized.lastDayExpenses < 0) normalized.lastDayExpenses = 0;

  if (Object.prototype.hasOwnProperty.call(business || {}, 'storageItemId')) {
    normalized.storageItemId = normalizeOptionalId(business?.storageItemId);
  }
  if (Object.prototype.hasOwnProperty.call(business || {}, 'cashDeskItemId')) {
    normalized.cashDeskItemId = normalizeOptionalId(business?.cashDeskItemId);
  }
  if (Object.prototype.hasOwnProperty.call(business || {}, 'shelfItemId')) {
    normalized.shelfItemId = normalizeOptionalId(business?.shelfItemId);
  }
  if (Object.prototype.hasOwnProperty.call(business || {}, 'hiredCashierContactId')) {
    normalized.hiredCashierContactId = normalizeOptionalId(business?.hiredCashierContactId);
  }
  if (Object.prototype.hasOwnProperty.call(business || {}, 'hiredMerchContactId')) {
    normalized.hiredMerchContactId = normalizeOptionalId(business?.hiredMerchContactId);
  }
  if (Object.prototype.hasOwnProperty.call(business || {}, 'hiredLogistContactId')) {
    normalized.hiredLogistContactId = normalizeOptionalId(business?.hiredLogistContactId);
  }

  return normalized;
}

function normalizeBusinessProfile(profile) {
  if (!profile) return profile;

  if (!Array.isArray(profile.businesses)) {
    profile.businesses = [];
  }

  if (!Array.isArray(profile.knownContacts)) {
    profile.knownContacts = [];
  }

  profile.knownContacts = profile.knownContacts.filter(id => typeof id === 'string' && id.trim().length > 0);

  profile.businesses = profile.businesses
    .map(normalizeBusinessInstance)
    .filter(business => business.instanceId && business.lotId);

  return profile;
}

function sanitizeBusinessProfile(profile, businessDefs) {
  if (!profile || !businessDefs) return profile;

  const businessTypeById = businessDefs.businessTypeById;
  const staffContactById = businessDefs.staffContactById;
  const traderItemById = businessDefs.traderItemById;
  const knownContacts = Array.isArray(profile.knownContacts) ? profile.knownContacts : [];
  const knownContactsSet = new Set(knownContacts);

  const seenLots = new Set();
  const seenInstances = new Set();
  const sanitized = [];
  const equippedItems = new Set();

  function resolveStorageCapacity(business) {
    if (!business?.storageItemId || !traderItemById) {
      return 0;
    }

    const item = traderItemById.get(business.storageItemId);
    return item && Number.isFinite(item.storageCapacity) ? Math.max(0, item.storageCapacity) : 0;
  }

  function resolveShelfCapacity(business) {
    if (!business?.shelfItemId || !traderItemById) {
      return 0;
    }

    const item = traderItemById.get(business.shelfItemId);
    return item && Number.isFinite(item.shelfCapacity) ? Math.max(0, item.shelfCapacity) : 0;
  }

  for (const business of profile.businesses || []) {
    if (!business || !business.instanceId || !business.lotId) continue;
    if (seenInstances.has(business.instanceId)) continue;
    if (seenLots.has(business.lotId)) continue;
    seenInstances.add(business.instanceId);
    seenLots.add(business.lotId);

    if (business.markupPercent < 0) business.markupPercent = 0;
    if (business.markupPercent > 100) business.markupPercent = 100;

    if (business.storageStock < 0) business.storageStock = 0;
    if (business.shelfStock < 0) business.shelfStock = 0;
    const storageCapacity = resolveStorageCapacity(business);
    const shelfCapacity = resolveShelfCapacity(business);
    if (storageCapacity > 0 && business.storageStock > storageCapacity) {
      business.storageStock = storageCapacity;
    }
    if (shelfCapacity > 0 && business.shelfStock > shelfCapacity) {
      business.shelfStock = shelfCapacity;
    }

    if (business.storageItemId && traderItemById && !traderItemById.has(business.storageItemId)) {
      business.storageItemId = null;
    }
    if (business.cashDeskItemId && traderItemById && !traderItemById.has(business.cashDeskItemId)) {
      business.cashDeskItemId = null;
    }
    if (business.shelfItemId && traderItemById && !traderItemById.has(business.shelfItemId)) {
      business.shelfItemId = null;
    }

    if (business.hiredCashierContactId) {
      const valid = staffContactById && staffContactById.has(business.hiredCashierContactId);
      if (!valid) {
        business.hiredCashierContactId = null;
      } else {
        knownContactsSet.add(business.hiredCashierContactId);
      }
    }
    if (business.hiredMerchContactId) {
      const valid = staffContactById && staffContactById.has(business.hiredMerchContactId);
      if (!valid) {
        business.hiredMerchContactId = null;
      } else {
        knownContactsSet.add(business.hiredMerchContactId);
      }
    }
    if (business.hiredLogistContactId) {
      const valid = staffContactById && staffContactById.has(business.hiredLogistContactId);
      if (!valid) {
        business.hiredLogistContactId = null;
      } else {
        knownContactsSet.add(business.hiredLogistContactId);
      }
    }

    if (business.isOpen) {
      if (!business.businessTypeId || (businessTypeById && !businessTypeById.has(business.businessTypeId))) {
        business.isOpen = false;
      } else {
        const hasRequiredEquipment =
          !!(business.storageItemId && String(business.storageItemId).trim()) &&
          !!(business.cashDeskItemId && String(business.cashDeskItemId).trim()) &&
          !!(business.shelfItemId && String(business.shelfItemId).trim());
        if (!hasRequiredEquipment) {
          business.isOpen = false;
        }
      }
    }

    if (business.storageItemId) equippedItems.add(business.storageItemId);
    if (business.cashDeskItemId) equippedItems.add(business.cashDeskItemId);
    if (business.shelfItemId) equippedItems.add(business.shelfItemId);

    sanitized.push(business);
  }

  profile.businesses = sanitized;
  profile.knownContacts = Array.from(knownContactsSet);
  if (!Array.isArray(profile.items)) {
    profile.items = [];
  }
  profile.items = profile.items.filter(itemId =>
    typeof itemId === 'string' &&
    itemId.trim().length > 0 &&
    !equippedItems.has(itemId));
  return profile;
}

function findBusinessByInstanceId(profile, instanceId) {
  if (!profile || !Array.isArray(profile.businesses) || !instanceId) return null;
  return profile.businesses.find(b => b.instanceId === instanceId) || null;
}

function findBusinessByLotId(profile, lotId) {
  if (!profile || !Array.isArray(profile.businesses) || !lotId) return null;
  return profile.businesses.find(b => b.lotId === lotId) || null;
}

module.exports = {
  normalizeBusinessProfile,
  sanitizeBusinessProfile,
  findBusinessByInstanceId,
  findBusinessByLotId
};
