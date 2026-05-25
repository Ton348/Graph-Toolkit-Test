const { findBusinessByLotId } = require('./businessState');
const { createBusinessInstance, applyBusinessTypeTemplate } = require('./businessInstanceFactory');

function ok(message) {
  return { ok: true, message: message || '' };
}

function fail(errorCode, message) {
  return { ok: false, errorCode, message: message || '' };
}

function requireLotId(lotId) {
  if (!lotId || !String(lotId).trim()) {
    return fail('LotIdEmpty', 'lotId is required.');
  }
  return null;
}

function normalizeOptionalId(value) {
  if (!value || typeof value !== 'string') {
    return null;
  }

  const trimmed = value.trim();
  return trimmed.length > 0 ? trimmed : null;
}

function removeKnownContact(profile, contactId) {
  if (!contactId) return;
  profile.knownContacts = Array.isArray(profile.knownContacts) ? profile.knownContacts : [];
  profile.knownContacts = profile.knownContacts.filter(id => id !== contactId);
}

function addKnownContact(profile, contactId) {
  if (!contactId) return;
  profile.knownContacts = Array.isArray(profile.knownContacts) ? profile.knownContacts : [];
  if (!profile.knownContacts.includes(contactId)) {
    profile.knownContacts.push(contactId);
  }
}

function resolveRentPerDay(business, lotDefs) {
  if (!business?.lotId || !lotDefs?.lotById) {
    return 0;
  }

  const lot = lotDefs.lotById.get(business.lotId);
  return lot && Number.isFinite(lot.rentPerDay) ? Math.max(0, lot.rentPerDay) : 0;
}

function resolveStorageCapacity(business, businessDefs) {
  if (!business?.storageItemId || !businessDefs?.traderItemById) {
    return 0;
  }

  const item = businessDefs.traderItemById.get(business.storageItemId);
  return item && Number.isFinite(item.storageCapacity) ? Math.max(0, item.storageCapacity) : 0;
}

function resolveShelfCapacity(business, businessDefs) {
  if (!business?.shelfItemId || !businessDefs?.traderItemById) {
    return 0;
  }

  const item = businessDefs.traderItemById.get(business.shelfItemId);
  return item && Number.isFinite(item.shelfCapacity) ? Math.max(0, item.shelfCapacity) : 0;
}

function resolveDeliveryPerDay(business, businessDefs) {
  const configuredLimit = Number.isFinite(business?.autoDeliveryPerDay)
    ? Math.max(0, Math.floor(business.autoDeliveryPerDay))
    : 0;
  if (configuredLimit <= 0) {
    return 0;
  }
  if (!business?.hiredLogistContactId || !businessDefs?.staffContactById) {
    return 0;
  }

  const logist = businessDefs.staffContactById.get(business.hiredLogistContactId);
  const throughputPerDay = logist && Number.isFinite(logist.throughputPerHour)
    ? Math.max(0, Math.floor(logist.throughputPerHour)) * 24
    : 0;
  return Math.min(configuredLimit, throughputPerDay);
}

function rentBusiness(profile, data, lotDefs, businessDefs) {
  const lotId = data && data.lotId;
  const lotCheck = requireLotId(lotId);
  if (lotCheck) return lotCheck;

  if (findBusinessByLotId(profile, lotId)) {
    return fail('BusinessAlreadyRented', 'Business already rented for this lot.');
  }

  const lot = lotDefs && lotDefs.lotById ? lotDefs.lotById.get(lotId) : null;
  if (!lot) {
    return fail('LotNotFound', 'Lot not found.');
  }

  const business = createBusinessInstance(
    businessDefs?.businessInstanceTemplate,
    lotId,
    '',
    businessDefs);
  profile.businesses.push(business);
  return ok('Rent business success.');
}

function assignBusinessType(profile, data, businessDefs, lotDefs) {
  const lotId = data && data.lotId;
  const businessTypeId = data && data.businessTypeId;
  const lotCheck = requireLotId(lotId);
  if (lotCheck) return lotCheck;
  if (!businessTypeId) return fail('BusinessTypeIdEmpty', 'businessTypeId is required.');

  const business = findBusinessByLotId(profile, lotId);
  if (!business) return fail('BusinessNotFound', 'Business not found.');

  const lot = lotDefs && lotDefs.lotById ? lotDefs.lotById.get(lotId) : null;
  if (!lot) return fail('LotNotFound', 'Lot not found.');
  if (Array.isArray(lot.allowedBusinessTypes) && lot.allowedBusinessTypes.length > 0 &&
      !lot.allowedBusinessTypes.includes(businessTypeId)) {
    return fail('BusinessTypeNotAllowedForLot', 'Business type not allowed for this lot.');
  }

  applyBusinessTypeTemplate(business, businessDefs?.businessInstanceTemplate, businessTypeId, businessDefs);
  return ok('Assign business type success.');
}

function installBusinessModule(profile, data, businessDefs) {
  return fail('BusinessModulesRemoved', 'Business modules are no longer supported. Use business equipment items instead.');
}

function assignSupplier(profile, data, businessDefs) {
  const lotId = data && data.lotId;
  const supplierId = data && typeof data.supplierId === 'string' ? data.supplierId.trim() : '';
  const lotCheck = requireLotId(lotId);
  if (lotCheck) return lotCheck;

  const business = findBusinessByLotId(profile, lotId);
  if (!business) return fail('BusinessNotFound', 'Business not found.');
  if (!supplierId) {
    addKnownContact(profile, business.hiredLogistContactId);
    business.hiredLogistContactId = null;
    business.autoDeliveryPerDay = 0;
    return ok('Clear supplier success.');
  }

  const supplierDef = businessDefs && businessDefs.supplierById && businessDefs.supplierById.get(supplierId);
  if (!supplierDef) return fail('SupplierNotFound', 'Supplier not found.');

  if (!Array.isArray(profile.knownContacts) || !profile.knownContacts.includes(supplierId)) {
    return fail('ContactNotKnown', 'Supplier contact not unlocked.');
  }

  if (business.hiredLogistContactId && business.hiredLogistContactId !== supplierId) {
    addKnownContact(profile, business.hiredLogistContactId);
  }
  business.hiredLogistContactId = supplierId;
  removeKnownContact(profile, supplierId);
  return ok('Assign supplier success.');
}

function hireBusinessWorker(profile, data, businessDefs) {
  const lotId = data && data.lotId;
  const roleId = data && data.roleId;
  const contactId = data && typeof data.contactId === 'string' ? data.contactId.trim() : '';
  const lotCheck = requireLotId(lotId);
  if (lotCheck) return lotCheck;
  if (!roleId) return fail('RoleIdEmpty', 'roleId is required.');

  const business = findBusinessByLotId(profile, lotId);
  if (!business) return fail('BusinessNotFound', 'Business not found.');

  if (!contactId) {
    if (roleId === 'cashier') {
      addKnownContact(profile, business.hiredCashierContactId);
      business.hiredCashierContactId = null;
      return ok('Clear cashier success.');
    }

    if (roleId === 'merchandiser') {
      addKnownContact(profile, business.hiredMerchContactId);
      business.hiredMerchContactId = null;
      return ok('Clear merchandiser success.');
    }
    if (roleId === 'logist') {
      addKnownContact(profile, business.hiredLogistContactId);
      business.hiredLogistContactId = null;
      business.autoDeliveryPerDay = 0;
      return ok('Clear logist success.');
    }

    return fail('InvalidWorkerRole', 'Unsupported worker role.');
  }

  if (!Array.isArray(profile.knownContacts) || !profile.knownContacts.includes(contactId)) {
    return fail('ContactNotKnown', 'Contact not unlocked.');
  }

  if (roleId === 'cashier') {
    if (business.hiredCashierContactId && business.hiredCashierContactId !== contactId) {
      addKnownContact(profile, business.hiredCashierContactId);
    }
    business.hiredCashierContactId = contactId;
  } else if (roleId === 'merchandiser') {
    if (business.hiredMerchContactId && business.hiredMerchContactId !== contactId) {
      addKnownContact(profile, business.hiredMerchContactId);
    }
    business.hiredMerchContactId = contactId;
  } else if (roleId === 'logist') {
    if (business.hiredLogistContactId && business.hiredLogistContactId !== contactId) {
      addKnownContact(profile, business.hiredLogistContactId);
    }
    business.hiredLogistContactId = contactId;
  } else {
    return fail('InvalidWorkerRole', 'Unsupported worker role.');
  }

  removeKnownContact(profile, contactId);
  return ok('Hire worker success.');
}

function openBusiness(profile, data, businessDefs) {
  const lotId = data && data.lotId;
  const lotCheck = requireLotId(lotId);
  if (lotCheck) return lotCheck;

  const business = findBusinessByLotId(profile, lotId);
  if (!business) return fail('BusinessNotFound', 'Business not found.');
  if (!business.businessTypeId) return fail('BusinessTypeMissing', 'Business type not assigned.');

  const hasRequiredEquipment =
    !!(business.storageItemId && String(business.storageItemId).trim()) &&
    !!(business.cashDeskItemId && String(business.cashDeskItemId).trim()) &&
    !!(business.shelfItemId && String(business.shelfItemId).trim());
  if (!hasRequiredEquipment) {
    return fail('MissingRequiredEquipment', 'Missing required equipment.');
  }

  business.isOpen = true;
  return ok('Open business success.');
}

function closeBusiness(profile, data) {
  const lotId = data && data.lotId;
  const lotCheck = requireLotId(lotId);
  if (lotCheck) return lotCheck;

  const business = findBusinessByLotId(profile, lotId);
  if (!business) return fail('BusinessNotFound', 'Business not found.');

  business.isOpen = false;
  return ok('Close business success.');
}

function setBusinessMarkup(profile, data) {
  const lotId = data && data.lotId;
  const markupPercent = data && data.markupPercent;
  const lotCheck = requireLotId(lotId);
  if (lotCheck) return lotCheck;
  if (!Number.isFinite(markupPercent) || markupPercent < 0 || markupPercent > 100) {
    return fail('InvalidMarkup', 'markupPercent must be between 0 and 100.');
  }

  const business = findBusinessByLotId(profile, lotId);
  if (!business) return fail('BusinessNotFound', 'Business not found.');

  business.markupPercent = markupPercent;
  return ok('Set markup success.');
}

function setBusinessAutoDelivery(profile, data) {
  const lotId = data && data.lotId;
  const dailyAmount = data && data.dailyAmount;
  const lotCheck = requireLotId(lotId);
  if (lotCheck) return lotCheck;

  if (!Number.isFinite(dailyAmount) || dailyAmount < 0) {
    return fail('InvalidDailyAmount', 'dailyAmount must be >= 0.');
  }

  const business = findBusinessByLotId(profile, lotId);
  if (!business) return fail('BusinessNotFound', 'Business not found.');

  business.autoDeliveryPerDay = Math.max(0, Math.floor(dailyAmount));
  return ok('Set auto delivery success.');
}


function simulateBusinessDay(profile, data, businessDefs, lotDefs) {
  const lotId = data && data.lotId;
  const lotCheck = requireLotId(lotId);
  if (lotCheck) return lotCheck;

  const business = findBusinessByLotId(profile, lotId);
  if (!business) return fail('BusinessNotFound', 'Business not found.');

  const currentPrice = Number.isFinite(business.markupPercent) ? Math.max(0, Math.floor(business.markupPercent)) : 0;
  const rentPerDay = resolveRentPerDay(business, lotDefs);
  const staffById = businessDefs?.staffContactById || null;
  const businessType = businessDefs?.businessTypeById && business.businessTypeId
    ? businessDefs.businessTypeById.get(business.businessTypeId)
    : null;
  const requiresMerch = businessType?.instanceTemplate?.hiredMerchContactId !== undefined &&
    businessType?.instanceTemplate?.hiredMerchContactId !== null;
  const cashier = staffById && business.hiredCashierContactId ? staffById.get(business.hiredCashierContactId) : null;
  const merch = staffById && business.hiredMerchContactId ? staffById.get(business.hiredMerchContactId) : null;
  const logist = staffById && business.hiredLogistContactId ? staffById.get(business.hiredLogistContactId) : null;
  const cashierSalary = cashier && Number.isFinite(cashier.salaryPerDay) ? Math.max(0, cashier.salaryPerDay) : 0;
  const merchSalary = merch && Number.isFinite(merch.salaryPerDay) ? Math.max(0, merch.salaryPerDay) : 0;
  const logistSalary = logist && Number.isFinite(logist.salaryPerDay) ? Math.max(0, logist.salaryPerDay) : 0;
  const hasStorageItem = !!(business.storageItemId && String(business.storageItemId).trim());
  const hasCashDeskItem = !!(business.cashDeskItemId && String(business.cashDeskItemId).trim());
  const hasShelfItem = !!(business.shelfItemId && String(business.shelfItemId).trim());
  const cashDeskItem = businessDefs?.traderItemById && business.cashDeskItemId
    ? businessDefs.traderItemById.get(business.cashDeskItemId)
    : null;
  const cashCapacity = cashDeskItem && Number.isFinite(cashDeskItem.cashCapacity) ? Math.max(0, cashDeskItem.cashCapacity) : 0;
  const hasRequiredStaff = cashier && (!requiresMerch || merch);
  const canSell = business.isOpen && hasCashDeskItem && hasShelfItem && hasRequiredStaff;
  const sold = 0;
  const revenue = Number.isFinite(business.dayRevenue) ? Math.max(0, Math.floor(business.dayRevenue)) : 0;
  const dayExpensesBase = rentPerDay + cashierSalary + (requiresMerch ? merchSalary : 0) + logistSalary;
  const deliveryExpenses = Number.isFinite(business.dayExpenses) ? Math.max(0, Math.floor(business.dayExpenses)) : 0;
  const dayExpenses = dayExpensesBase + deliveryExpenses;
  const dayProfitDelta = revenue - dayExpenses;
  business.dayExpenses = dayExpenses;
  business.profit = (Number.isFinite(business.profit) ? business.profit : 0) + dayProfitDelta;
  business.dayRevenue = 0;
  business.dayExpenses = 0;
  if (cashCapacity > 0 && business.profit > cashCapacity) {
    business.profit = cashCapacity;
  }

  return ok('Simulate business day success.');
}

function applyBusinessDelivery(profile, data, businessDefs) {
  const lotId = data && data.lotId;
  const requestedAmount = data && data.requestedAmount;
  const lotCheck = requireLotId(lotId);
  if (lotCheck) return lotCheck;
  if (!Number.isFinite(requestedAmount) || requestedAmount <= 0) {
    return fail('AmountInvalid', 'requestedAmount must be > 0.');
  }

  const business = findBusinessByLotId(profile, lotId);
  if (!business) return fail('BusinessNotFound', 'Business not found.');
  if (!business.isOpen) return fail('BusinessClosed', 'Business is closed.');

  const storageCapacity = resolveStorageCapacity(business, businessDefs);
  const storageStock = Number.isFinite(business.storageStock) ? Math.max(0, Math.floor(business.storageStock)) : 0;
  const freeStorage = Math.max(0, storageCapacity - storageStock);
  const deliveryLimit = resolveDeliveryPerDay(business, businessDefs);
  const orderedByLogist = Math.min(Math.max(0, Math.floor(requestedAmount)), deliveryLimit);
  const addedToStorage = Math.min(orderedByLogist, freeStorage);
  const supplier = businessDefs?.supplierById && business.hiredLogistContactId
    ? businessDefs.supplierById.get(business.hiredLogistContactId)
    : null;
  const unitCost = supplier && Number.isFinite(supplier.unitBuyPrice) ? Math.max(0, supplier.unitBuyPrice) : 0;
  const deliveryCost = orderedByLogist * unitCost;

  business.storageStock = storageStock + addedToStorage;
  business.dayExpenses = (Number.isFinite(business.dayExpenses) ? Math.max(0, Math.floor(business.dayExpenses)) : 0) + deliveryCost;
  return ok('Business delivery applied.');
}

function collectBusinessProfit(profile, data, businessDefs) {
  const lotId = data && data.lotId;
  const lotCheck = requireLotId(lotId);
  if (lotCheck) return lotCheck;

  const business = findBusinessByLotId(profile, lotId);
  if (!business) return fail('BusinessNotFound', 'Business not found.');

  const cashDeskItem = businessDefs?.traderItemById && business.cashDeskItemId
    ? businessDefs.traderItemById.get(business.cashDeskItemId)
    : null;
  const cashCapacity = cashDeskItem && Number.isFinite(cashDeskItem.cashCapacity) ? Math.max(0, cashDeskItem.cashCapacity) : 0;
  if (cashCapacity > 0 && business.profit > cashCapacity) {
    business.profit = cashCapacity;
  }

  const amount = Number.isFinite(business.profit) ? Math.max(0, business.profit) : 0;
  if (amount <= 0) {
    return fail('NoProfitToCollect', 'No positive profit to collect.');
  }

  profile.money = (Number.isFinite(profile.money) ? profile.money : 0) + amount;
  business.profit = 0;
  return ok('Collect business profit success.');
}

function consumeNpcService(profile, data, businessDefs) {
  const lotId = data && data.lotId;
  const serviceId = data && typeof data.serviceId === 'string' ? data.serviceId.trim() : '';
  const requestedAmount = data && Number.isFinite(data.requestedAmount) ? Math.max(0, Math.floor(data.requestedAmount)) : 0;
  const lotCheck = requireLotId(lotId);
  if (lotCheck) return lotCheck;
  if (!serviceId) return fail('ServiceIdEmpty', 'serviceId is required.');
  if (requestedAmount <= 0) return fail('InvalidAmount', 'requestedAmount must be > 0.');

  const business = findBusinessByLotId(profile, lotId);
  if (!business) return fail('BusinessNotFound', 'Business not found.');
  if (!business.isOpen) return fail('BusinessClosed', 'Business is closed.');

  if (serviceId !== 'Food' && serviceId !== 'Drink') {
    return ok('NPC service success.');
  }

  if (!business.hiredCashierContactId || !String(business.hiredCashierContactId).trim()) {
    return fail('NoCashier', 'No active cashier.');
  }

  const shelfStock = Number.isFinite(business.shelfStock) ? Math.max(0, Math.floor(business.shelfStock)) : 0;
  if (shelfStock <= 0) {
    return fail('NoShelfStock', 'No shelf stock.');
  }

  const consumed = shelfStock < requestedAmount ? shelfStock : requestedAmount;
  business.shelfStock = shelfStock - consumed;

  const itemPrice = Number.isFinite(business.markupPercent) ? Math.max(0, Math.floor(business.markupPercent)) : 0;
  const price = consumed * itemPrice;
  if (price > 0) {
    business.dayRevenue = (Number.isFinite(business.dayRevenue) ? business.dayRevenue : 0) + price;
    business.profit = (Number.isFinite(business.profit) ? business.profit : 0) + price;
  }

  return ok('NPC consume success.');
}

function unlockContact(profile, data) {
  const contactId = data && data.contactId;
  if (!contactId || !String(contactId).trim()) {
    return fail('ContactIdEmpty', 'contactId is required.');
  }

  profile.knownContacts = Array.isArray(profile.knownContacts) ? profile.knownContacts : [];
  if (!profile.knownContacts.includes(contactId)) {
    profile.knownContacts.push(contactId);
  }

  return ok('Unlock contact success.');
}


function moveBusinessStockToShelf(profile, data, businessDefs) {
  const lotId = data && data.lotId;
  const amount = data && data.amount;
  const lotCheck = requireLotId(lotId);
  if (lotCheck) return lotCheck;
  if (!Number.isFinite(amount) || amount <= 0) {
    return fail('AmountInvalid', 'amount must be positive.');
  }

  const business = findBusinessByLotId(profile, lotId);
  if (!business) return fail('BusinessNotFound', 'Business not found.');

  const shelfCapacity = resolveShelfCapacity(business, businessDefs);
  const shelfStock = Number.isFinite(business.shelfStock) ? Math.max(0, business.shelfStock) : 0;
  const storageStock = Number.isFinite(business.storageStock) ? Math.max(0, business.storageStock) : 0;
  const freeShelf = Math.max(0, shelfCapacity - shelfStock);
  const moved = Math.min(Math.floor(amount), Math.min(freeShelf, storageStock));
  if (moved <= 0) {
    return ok('Move stock success.');
  }

  business.storageStock = storageStock - moved;
  business.shelfStock = shelfStock + moved;
  return ok('Move stock success.');
}

function validateEquipmentItem(profile, businessDefs, itemId, requiredCategory) {
  if (!itemId) {
    return ok();
  }

  const ownedItems = Array.isArray(profile.items) ? profile.items : [];
  if (!ownedItems.includes(itemId)) {
    return fail('ItemNotOwned', `Item '${itemId}' is not owned.`);
  }

  const item = businessDefs?.traderItemById ? businessDefs.traderItemById.get(itemId) : null;
  if (!item) {
    return fail('ItemNotFound', `Item '${itemId}' not found.`);
  }

  const itemCategory = resolveEquipmentSlotKey(itemId, item.category);
  const requiredSlot = resolveEquipmentSlotKey(requiredCategory, requiredCategory);
  if (itemCategory !== requiredSlot) {
    return fail('InvalidItemCategory', `Item '${itemId}' is not category '${requiredCategory}'.`);
  }

  return ok();
}

function normalizeEquipmentCategory(category) {
  if (!category || typeof category !== 'string') return null;
  const trimmed = category.trim().toLowerCase();
  if (trimmed.startsWith('storage')) return 'storage';
  if (trimmed.startsWith('cashdesk')) return 'cashdesk';
  if (trimmed.startsWith('shelf')) return 'shelf';
  return trimmed;
}

function resolveEquipmentSlotKey(itemId, category) {
  const normalizedCategory = normalizeEquipmentCategory(category);
  if (normalizedCategory) {
    return normalizedCategory;
  }

  return normalizeEquipmentCategory(itemId);
}

function setBusinessEquipment(profile, data, businessDefs) {
  const lotId = data && data.lotId;
  const lotCheck = requireLotId(lotId);
  if (lotCheck) return lotCheck;

  const business = findBusinessByLotId(profile, lotId);
  if (!business) return fail('BusinessNotFound', 'Business not found.');

  const storageItemId = normalizeOptionalId(data && data.storageItemId);
  const cashDeskItemId = normalizeOptionalId(data && data.cashDeskItemId);
  const shelfItemId = normalizeOptionalId(data && data.shelfItemId);
  const currentStorageItemId = normalizeOptionalId(business.storageItemId);
  const currentCashDeskItemId = normalizeOptionalId(business.cashDeskItemId);
  const currentShelfItemId = normalizeOptionalId(business.shelfItemId);
  profile.items = Array.isArray(profile.items) ? profile.items : [];

  let validation = validateEquipmentItem(
    profile,
    businessDefs,
    storageItemId !== currentStorageItemId ? storageItemId : null,
    'storage');
  if (!validation.ok) return validation;
  validation = validateEquipmentItem(
    profile,
    businessDefs,
    cashDeskItemId !== currentCashDeskItemId ? cashDeskItemId : null,
    'cashdesk');
  if (!validation.ok) return validation;
  validation = validateEquipmentItem(
    profile,
    businessDefs,
    shelfItemId !== currentShelfItemId ? shelfItemId : null,
    'shelf');
  if (!validation.ok) return validation;

  if (storageItemId !== currentStorageItemId) {
    if (storageItemId) {
      profile.items = profile.items.filter(id => id !== storageItemId);
    }
    if (currentStorageItemId && !profile.items.includes(currentStorageItemId)) {
      profile.items.push(currentStorageItemId);
    }
  }

  if (cashDeskItemId !== currentCashDeskItemId) {
    if (cashDeskItemId) {
      profile.items = profile.items.filter(id => id !== cashDeskItemId);
    }
    if (currentCashDeskItemId && !profile.items.includes(currentCashDeskItemId)) {
      profile.items.push(currentCashDeskItemId);
    }
  }

  if (shelfItemId !== currentShelfItemId) {
    if (shelfItemId) {
      profile.items = profile.items.filter(id => id !== shelfItemId);
    }
    if (currentShelfItemId && !profile.items.includes(currentShelfItemId)) {
      profile.items.push(currentShelfItemId);
    }
  }

  business.storageItemId = storageItemId;
  business.cashDeskItemId = cashDeskItemId;
  business.shelfItemId = shelfItemId;

  const storageCapacity = resolveStorageCapacity(business, businessDefs);
  const shelfCapacity = resolveShelfCapacity(business, businessDefs);

  if (business.storageStock > storageCapacity) {
    business.storageStock = storageCapacity;
  }

  if (business.shelfStock > shelfCapacity) {
    business.shelfStock = shelfCapacity;
  }

  return ok('Set business equipment success.');
}

function buyItem(profile, data, businessDefs) {
  const traderId = data && data.traderId;
  const itemId = data && data.itemId;
  const lotId = data && data.lotId;

  if (!traderId || !String(traderId).trim()) {
    return fail('TraderIdEmpty', 'traderId is required.');
  }

  if (!itemId || !String(itemId).trim()) {
    return fail('ItemIdEmpty', 'itemId is required.');
  }

  const trader = businessDefs?.traderById ? businessDefs.traderById.get(traderId) : null;
  if (!trader) {
    return fail('TraderNotFound', 'Trader not found.');
  }

  const item = businessDefs?.traderItemById ? businessDefs.traderItemById.get(itemId) : null;
  if (!item) {
    return fail('ItemNotFound', 'Item not found.');
  }

  const soldByTrader = Array.isArray(trader.itemIds) && trader.itemIds.some(id => id === itemId);
  if (!soldByTrader) {
    return fail('ItemNotSoldByTrader', 'Item is not sold by this trader.');
  }

  profile.items = Array.isArray(profile.items) ? profile.items : [];
  if (profile.items.includes(itemId)) {
    return fail('ItemAlreadyOwned', 'Item already owned.');
  }

  const price = Number.isFinite(item.price) ? item.price : 0;
  if (!Number.isFinite(profile.money) || profile.money < price) {
    return fail('NotEnoughMoney', 'Not enough money.');
  }

  profile.money -= price;
  profile.items.push(itemId);

  return ok('Buy item success.');
}

function getTraderItems(data, businessDefs) {
  const traderId = data && data.traderId;
  if (!traderId || !String(traderId).trim()) {
    return fail('TraderIdEmpty', 'traderId is required.');
  }

  const trader = businessDefs?.traderById ? businessDefs.traderById.get(traderId) : null;
  if (!trader) {
    return fail('TraderNotFound', 'Trader not found.');
  }

  return {
    ok: true,
    message: 'Get trader items success.',
    traderId: trader.id,
    traderName: trader.name,
    items: Array.isArray(trader.itemIds)
      ? trader.itemIds
        .map(itemId => businessDefs?.traderItemById ? businessDefs.traderItemById.get(itemId) : null)
        .filter(Boolean)
      : []
  };
}

function resetBusinesses(profile) {
  profile.businesses = [];
  return ok('Businesses reset.');
}

module.exports = {
  rentBusiness,
  assignBusinessType,
  installBusinessModule,
  assignSupplier,
  setBusinessEquipment,
  buyItem,
  getTraderItems,
  hireBusinessWorker,
  openBusiness,
  closeBusiness,
  setBusinessMarkup,
  setBusinessAutoDelivery,
  simulateBusinessDay,
  applyBusinessDelivery,
  unlockContact,
  moveBusinessStockToShelf,
  collectBusinessProfit,
  consumeNpcService,
  resetBusinesses,
};
