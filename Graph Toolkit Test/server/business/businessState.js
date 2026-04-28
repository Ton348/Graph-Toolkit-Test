function normalizeBusinessInstance(business) {
  const normalized = {
    instanceId: typeof business?.instanceId === 'string' ? business.instanceId : '',
    lotId: typeof business?.lotId === 'string' ? business.lotId : '',
    businessTypeId: typeof business?.businessTypeId === 'string' ? business.businessTypeId : '',
    isOpen: Boolean(business?.isOpen),
    rentPerDay: Number.isFinite(business?.rentPerDay) ? business.rentPerDay : 0,
    storageCapacity: Number.isFinite(business?.storageCapacity) ? business.storageCapacity : 0,
    shelfCapacity: Number.isFinite(business?.shelfCapacity) ? business.shelfCapacity : 0,
    storageStock: Number.isFinite(business?.storageStock) ? business.storageStock : 0,
    shelfStock: Number.isFinite(business?.shelfStock) ? business.shelfStock : 0,
    selectedSupplierId: typeof business?.selectedSupplierId === 'string' ? business.selectedSupplierId : null,
    autoDeliveryPerDay: Number.isFinite(business?.autoDeliveryPerDay) ? business.autoDeliveryPerDay : 0,
    markupPercent: Number.isFinite(business?.markupPercent) ? business.markupPercent : 0,
    lastDayRevenue: Number.isFinite(business?.lastDayRevenue) ? business.lastDayRevenue : 0,
    lastDayExpenses: Number.isFinite(business?.lastDayExpenses) ? business.lastDayExpenses : 0,
    lastDayProfit: Number.isFinite(business?.lastDayProfit) ? business.lastDayProfit : 0,
    totalRevenue: Number.isFinite(business?.totalRevenue) ? business.totalRevenue : 0,
    totalExpenses: Number.isFinite(business?.totalExpenses) ? business.totalExpenses : 0,
    totalProfit: Number.isFinite(business?.totalProfit) ? business.totalProfit : 0
  };

  if (normalized.rentPerDay < 0) normalized.rentPerDay = 0;
  if (normalized.storageCapacity < 0) normalized.storageCapacity = 0;
  if (normalized.shelfCapacity < 0) normalized.shelfCapacity = 0;
  if (normalized.storageStock < 0) normalized.storageStock = 0;
  if (normalized.shelfStock < 0) normalized.shelfStock = 0;
  if (normalized.autoDeliveryPerDay < 0) normalized.autoDeliveryPerDay = 0;
  if (normalized.markupPercent < 0) normalized.markupPercent = 0;
  if (normalized.lastDayRevenue < 0) normalized.lastDayRevenue = 0;
  if (normalized.lastDayExpenses < 0) normalized.lastDayExpenses = 0;

  if (Object.prototype.hasOwnProperty.call(business || {}, 'storageItemId')) {
    normalized.storageItemId = typeof business?.storageItemId === 'string' ? business.storageItemId : null;
  }
  if (Object.prototype.hasOwnProperty.call(business || {}, 'cashDeskItemId')) {
    normalized.cashDeskItemId = typeof business?.cashDeskItemId === 'string' ? business.cashDeskItemId : null;
  }
  if (Object.prototype.hasOwnProperty.call(business || {}, 'shelfItemId')) {
    normalized.shelfItemId = typeof business?.shelfItemId === 'string' ? business.shelfItemId : null;
  }
  if (Object.prototype.hasOwnProperty.call(business || {}, 'hiredCashierContactId')) {
    normalized.hiredCashierContactId = typeof business?.hiredCashierContactId === 'string' ? business.hiredCashierContactId : null;
  }
  if (Object.prototype.hasOwnProperty.call(business || {}, 'hiredMerchContactId')) {
    normalized.hiredMerchContactId = typeof business?.hiredMerchContactId === 'string' ? business.hiredMerchContactId : null;
  }
  if (Object.prototype.hasOwnProperty.call(business || {}, 'hiredLogistContactId')) {
    normalized.hiredLogistContactId = typeof business?.hiredLogistContactId === 'string' ? business.hiredLogistContactId : null;
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

  const supplierById = businessDefs.supplierById;
  const businessTypeById = businessDefs.businessTypeById;
  const knownContacts = Array.isArray(profile.knownContacts) ? profile.knownContacts : [];

  const seenLots = new Set();
  const seenInstances = new Set();
  const sanitized = [];
  const equippedItems = new Set();

  for (const business of profile.businesses || []) {
    if (!business || !business.instanceId || !business.lotId) continue;
    if (seenInstances.has(business.instanceId)) continue;
    if (seenLots.has(business.lotId)) continue;
    seenInstances.add(business.instanceId);
    seenLots.add(business.lotId);

    if (business.markupPercent < 0) business.markupPercent = 0;
    if (business.markupPercent > 100) business.markupPercent = 100;

    if (business.storageCapacity < 0) business.storageCapacity = 0;
    if (business.shelfCapacity < 0) business.shelfCapacity = 0;
    if (business.storageStock < 0) business.storageStock = 0;
    if (business.shelfStock < 0) business.shelfStock = 0;
    if (business.storageCapacity > 0 && business.storageStock > business.storageCapacity) {
      business.storageStock = business.storageCapacity;
    }
    if (business.shelfCapacity > 0 && business.shelfStock > business.shelfCapacity) {
      business.shelfStock = business.shelfCapacity;
    }

    if (business.selectedSupplierId) {
      if ((supplierById && !supplierById.has(business.selectedSupplierId)) || !knownContacts.includes(business.selectedSupplierId)) {
        business.selectedSupplierId = null;
      }
    }

    if (business.hiredCashierContactId && !knownContacts.includes(business.hiredCashierContactId)) {
      business.hiredCashierContactId = null;
    }
    if (business.hiredMerchContactId && !knownContacts.includes(business.hiredMerchContactId)) {
      business.hiredMerchContactId = null;
    }
    if (business.hiredLogistContactId && !knownContacts.includes(business.hiredLogistContactId)) {
      business.hiredLogistContactId = null;
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
