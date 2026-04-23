const { findBusinessByLotId } = require('./businessState');

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

function createBusinessInstance(lotId, rentPerDay) {
  const instanceId = `biz_${Date.now()}_${Math.floor(Math.random() * 10000)}`;
  return {
    instanceId,
    lotId,
    businessTypeId: null,
    isOpen: false,
    rentPerDay: Number.isFinite(rentPerDay) && rentPerDay >= 0 ? rentPerDay : 0,
    storageCapacity: 0,
    shelfCapacity: 0,
    storageStock: 0,
    shelfStock: 0,
    storageItemId: null,
    cashDeskItemId: null,
    shelfItemId: null,
    selectedSupplierId: null,
    autoDeliveryPerDay: 0,
    markupPercent: 0,
    hiredCashierContactId: null,
    hiredMerchContactId: null,
    hiredLogistContactId: null,
    lastDayRevenue: 0,
    lastDayExpenses: 0,
    lastDayProfit: 0,
    totalRevenue: 0,
    totalExpenses: 0,
    totalProfit: 0
  };
}

function rentBusiness(profile, data, lotDefs) {
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

  const business = createBusinessInstance(lotId, lot.rentPerDay);
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

  const typeDef = businessDefs && businessDefs.businessTypeById && businessDefs.businessTypeById.get(businessTypeId);
  if (!typeDef) return fail('BusinessTypeNotFound', 'Business type not found.');

  business.businessTypeId = businessTypeId;
  business.storageCapacity = Number.isFinite(typeDef.defaultStorageCapacity) ? typeDef.defaultStorageCapacity : 0;
  business.shelfCapacity = Number.isFinite(typeDef.defaultShelfCapacity) ? typeDef.defaultShelfCapacity : 0;

  return ok('Assign business type success.');
}

function installBusinessModule(profile, data, businessDefs) {
  const lotId = data && data.lotId;
  const moduleId = data && data.moduleId;
  const lotCheck = requireLotId(lotId);
  if (lotCheck) return lotCheck;
  if (!moduleId) return fail('ModuleIdEmpty', 'moduleId is required.');

  const business = findBusinessByLotId(profile, lotId);
  if (!business) return fail('BusinessNotFound', 'Business not found.');

  const moduleDef = businessDefs && businessDefs.moduleById && businessDefs.moduleById.get(moduleId);
  if (!moduleDef) return fail('ModuleNotFound', 'Module not found.');

  const cost = Number.isFinite(moduleDef.installCost) ? moduleDef.installCost : 0;
  business.totalProfit = (Number.isFinite(business.totalProfit) ? business.totalProfit : 0) - cost;
  return ok('Install module success.');
}

function assignSupplier(profile, data, businessDefs) {
  const lotId = data && data.lotId;
  const supplierId = data && typeof data.supplierId === 'string' ? data.supplierId.trim() : '';
  const lotCheck = requireLotId(lotId);
  if (lotCheck) return lotCheck;

  const business = findBusinessByLotId(profile, lotId);
  if (!business) return fail('BusinessNotFound', 'Business not found.');
  if (!supplierId) {
    business.selectedSupplierId = null;
    business.autoDeliveryPerDay = 0;
    return ok('Clear supplier success.');
  }

  const supplierDef = businessDefs && businessDefs.supplierById && businessDefs.supplierById.get(supplierId);
  if (!supplierDef) return fail('SupplierNotFound', 'Supplier not found.');

  if (!Array.isArray(profile.knownContacts) || !profile.knownContacts.includes(supplierId)) {
    return fail('ContactNotKnown', 'Supplier contact not unlocked.');
  }

  business.selectedSupplierId = supplierId;
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
      business.hiredCashierContactId = null;
      return ok('Clear cashier success.');
    }

    if (roleId === 'merchandiser') {
      business.hiredMerchContactId = null;
      return ok('Clear merchandiser success.');
    }
    if (roleId === 'logist') {
      business.hiredLogistContactId = null;
      business.selectedSupplierId = null;
      business.autoDeliveryPerDay = 0;
      return ok('Clear logist success.');
    }

    return fail('InvalidWorkerRole', 'Unsupported worker role.');
  }

  if (!Array.isArray(profile.knownContacts) || !profile.knownContacts.includes(contactId)) {
    return fail('ContactNotKnown', 'Contact not unlocked.');
  }

  if (roleId === 'cashier') {
    business.hiredCashierContactId = contactId;
  } else if (roleId === 'merchandiser') {
    business.hiredMerchContactId = contactId;
  } else if (roleId === 'logist') {
    business.hiredLogistContactId = contactId;
    business.selectedSupplierId = contactId;
  } else {
    return fail('InvalidWorkerRole', 'Unsupported worker role.');
  }

  return ok('Hire worker success.');
}

function openBusiness(profile, data, businessDefs) {
  const lotId = data && data.lotId;
  const lotCheck = requireLotId(lotId);
  if (lotCheck) return lotCheck;

  const business = findBusinessByLotId(profile, lotId);
  if (!business) return fail('BusinessNotFound', 'Business not found.');
  if (!business.businessTypeId) return fail('BusinessTypeMissing', 'Business type not assigned.');

  const typeDef = businessDefs && businessDefs.businessTypeById && businessDefs.businessTypeById.get(business.businessTypeId);
  if (!typeDef) return fail('BusinessTypeNotFound', 'Business type not found.');

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

  business.autoDeliveryPerDay = Math.floor(dailyAmount);
  return ok('Set auto delivery success.');
}

function resolveDailyDemand(ranges, currentPrice) {
  if (!Array.isArray(ranges) || !Number.isFinite(currentPrice)) {
    return 0;
  }

  for (const range of ranges) {
    if (!range) {
      continue;
    }

    if (currentPrice >= range.minPrice && currentPrice <= range.maxPrice) {
      return Number.isFinite(range.dailyDemand) ? Math.max(0, Math.floor(range.dailyDemand)) : 0;
    }
  }

  return 0;
}

function simulateBusinessDay(profile, data, businessDefs) {
  const lotId = data && data.lotId;
  const lotCheck = requireLotId(lotId);
  if (lotCheck) return lotCheck;

  const business = findBusinessByLotId(profile, lotId);
  if (!business) return fail('BusinessNotFound', 'Business not found.');

  const demandRanges = businessDefs?.demandByBusinessTypeId
    ? businessDefs.demandByBusinessTypeId.get(business.businessTypeId)
    : null;

  const currentPrice = Number.isFinite(business.markupPercent) ? Math.max(0, Math.floor(business.markupPercent)) : 0;
  const stock = Number.isFinite(business.storageStock) ? Math.max(0, Math.floor(business.storageStock)) : 0;
  const storageCapacity = Number.isFinite(business.storageCapacity) ? Math.max(0, Math.floor(business.storageCapacity)) : 0;
  const dailyOrderAmount = Number.isFinite(business.autoDeliveryPerDay)
    ? Math.max(0, Math.floor(business.autoDeliveryPerDay))
    : 0;

  const supplier = businessDefs?.supplierById && business.selectedSupplierId
    ? businessDefs.supplierById.get(business.selectedSupplierId)
    : null;
  const unitCost = supplier && Number.isFinite(supplier.unitBuyPrice) ? Math.max(0, supplier.unitBuyPrice) : 0;
  const rentPerDay = Number.isFinite(business.rentPerDay) ? Math.max(0, business.rentPerDay) : 0;
  const staffById = businessDefs?.staffContactById || null;
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
  const canDeliver = hasStorageItem && supplier && dailyOrderAmount > 0;
  const canSell = business.isOpen && hasCashDeskItem && hasShelfItem && cashier && merch;

  const storageFreeSpace = Math.max(0, storageCapacity - stock);
  const delivered = canDeliver ? Math.min(dailyOrderAmount, storageFreeSpace) : 0;
  const stockAfterDelivery = stock + delivered;
  const dailyDemand = resolveDailyDemand(demandRanges, currentPrice);
  const sold = canSell ? Math.min(dailyDemand, stockAfterDelivery) : 0;
  const revenue = sold * currentPrice;
  const deliveryCost = canDeliver ? delivered * unitCost : 0;
  const totalExpenses = deliveryCost + rentPerDay + cashierSalary + merchSalary + logistSalary;
  const profit = revenue - totalExpenses;
  const stockEnd = Math.max(0, stockAfterDelivery - sold);

  business.storageStock = stockEnd;
  business.lastDayRevenue = revenue;
  business.lastDayExpenses = totalExpenses;
  business.lastDayProfit = profit;
  business.totalRevenue = (Number.isFinite(business.totalRevenue) ? business.totalRevenue : 0) + revenue;
  business.totalExpenses = (Number.isFinite(business.totalExpenses) ? business.totalExpenses : 0) + totalExpenses;
  business.totalProfit = (Number.isFinite(business.totalProfit) ? business.totalProfit : 0) + profit;
  if (cashCapacity > 0 && business.totalProfit > cashCapacity) {
    business.totalProfit = cashCapacity;
  }

  return ok('Simulate business day success.');
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
  if (cashCapacity > 0 && business.totalProfit > cashCapacity) {
    business.totalProfit = cashCapacity;
  }

  const amount = Number.isFinite(business.totalProfit) ? Math.max(0, business.totalProfit) : 0;
  if (amount <= 0) {
    return fail('NoProfitToCollect', 'No positive profit to collect.');
  }

  profile.money = (Number.isFinite(profile.money) ? profile.money : 0) + amount;
  business.totalProfit = 0;
  return ok('Collect business profit success.');
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

function addBusinessStock(profile, data) {
  const lotId = data && data.lotId;
  const amount = data && data.amount;
  const lotCheck = requireLotId(lotId);
  if (lotCheck) return lotCheck;

  if (!Number.isFinite(amount) || amount <= 0) {
    return fail('AmountInvalid', 'amount must be positive.');
  }

  const business = findBusinessByLotId(profile, lotId);
  if (!business) return fail('BusinessNotFound', 'Business not found.');

  if (!business.storageItemId) {
    return fail('StorageMissing', 'Storage equipment not installed.');
  }

  const capacity = Number.isFinite(business.storageCapacity) ? business.storageCapacity : 0;
  const current = Number.isFinite(business.storageStock) ? business.storageStock : 0;
  const space = capacity - current;
  if (space <= 0) {
    return fail('StorageFull', 'Storage is full.');
  }

  const added = amount > space ? space : amount;
  business.storageStock = current + added;
  return ok(`Added stock: ${added}.`);
}

function addBusinessShelfStock(profile, data) {
  const lotId = data && data.lotId;
  const amount = data && data.amount;
  const lotCheck = requireLotId(lotId);
  if (lotCheck) return lotCheck;

  if (!Number.isFinite(amount) || amount <= 0) {
    return fail('AmountInvalid', 'amount must be positive.');
  }

  const business = findBusinessByLotId(profile, lotId);
  if (!business) return fail('BusinessNotFound', 'Business not found.');

  if (!business.shelfItemId) {
    return fail('ShelvesMissing', 'Shelves equipment not installed.');
  }

  const capacity = Number.isFinite(business.shelfCapacity) ? business.shelfCapacity : 0;
  const current = Number.isFinite(business.shelfStock) ? business.shelfStock : 0;
  const space = capacity - current;
  if (space <= 0) {
    return fail('ShelvesFull', 'Shelves are full.');
  }

  const added = amount > space ? space : amount;
  business.shelfStock = current + added;
  return ok(`Added shelf stock: ${added}.`);
}

function clearBusinessStock(profile, data) {
  const lotId = data && data.lotId;
  const lotCheck = requireLotId(lotId);
  if (lotCheck) return lotCheck;

  const business = findBusinessByLotId(profile, lotId);
  if (!business) return fail('BusinessNotFound', 'Business not found.');

  business.storageStock = 0;
  business.shelfStock = 0;
  return ok('Cleared business stock.');
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

  const storageItem = storageItemId && businessDefs?.traderItemById
    ? businessDefs.traderItemById.get(storageItemId)
    : null;
  const shelfItem = shelfItemId && businessDefs?.traderItemById
    ? businessDefs.traderItemById.get(shelfItemId)
    : null;

  business.storageCapacity = storageItem && Number.isFinite(storageItem.storageCapacity)
    ? storageItem.storageCapacity
    : 0;
  business.shelfCapacity = shelfItem && Number.isFinite(shelfItem.shelfCapacity)
    ? shelfItem.shelfCapacity
    : 0;

  if (business.storageStock > business.storageCapacity) {
    business.storageStock = business.storageCapacity;
  }

  if (business.shelfStock > business.shelfCapacity) {
    business.shelfStock = business.shelfCapacity;
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

  const soldByTrader = Array.isArray(trader.items) && trader.items.some(t => t && t.id === itemId);
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

  if (lotId && String(lotId).trim()) {
    const business = findBusinessByLotId(profile, lotId);
    if (business) {
      if (item.category === 'storage') {
        business.storageItemId = item.id;
        business.storageCapacity = Number.isFinite(item.storageCapacity) ? item.storageCapacity : 0;
        if (business.storageStock > business.storageCapacity) {
          business.storageStock = business.storageCapacity;
        }
      } else if (item.category === 'cashdesk') {
        business.cashDeskItemId = item.id;
      } else if (item.category === 'shelf') {
        business.shelfItemId = item.id;
        business.shelfCapacity = Number.isFinite(item.shelfCapacity) ? item.shelfCapacity : 0;
        if (business.shelfStock > business.shelfCapacity) {
          business.shelfStock = business.shelfCapacity;
        }
      }
    }
  }

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
    items: Array.isArray(trader.items) ? trader.items : []
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
  unlockContact,
  addBusinessStock,
  addBusinessShelfStock,
  clearBusinessStock,
  collectBusinessProfit,
  resetBusinesses,
};
