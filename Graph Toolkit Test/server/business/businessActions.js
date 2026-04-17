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

function createBusinessInstance(lotId, rentPerDay) {
  const instanceId = `biz_${Date.now()}_${Math.floor(Math.random() * 10000)}`;
  return {
    instanceId,
    lotId,
    businessTypeId: null,
    isOpen: false,
    rentPerDay: Number.isFinite(rentPerDay) && rentPerDay >= 0 ? rentPerDay : 0,
    installedModules: [],
    storageCapacity: 0,
    shelfCapacity: 0,
    storageStock: 0,
    shelfStock: 0,
    selectedSupplierId: null,
    autoDeliveryPerDay: 0,
    markupPercent: 0,
    hiredCashierContactId: null,
    hiredMerchContactId: null,
    hiredLogistContactId: null
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

  if (Array.isArray(business.installedModules) && business.installedModules.includes(moduleId)) {
    return fail('ModuleAlreadyInstalled', 'Module already installed.');
  }

  const cost = Number.isFinite(moduleDef.installCost) ? moduleDef.installCost : 0;
  if (profile.money < cost) {
    return fail('NotEnoughMoney', 'Not enough money.');
  }

  profile.money -= cost;
  business.installedModules = Array.isArray(business.installedModules) ? business.installedModules : [];
  business.installedModules.push(moduleId);
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

  const roleDef = businessDefs && businessDefs.staffRoleById && businessDefs.staffRoleById.get(roleId);
  if (!roleDef) return fail('InvalidWorkerRole', 'Worker role not found.');
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
      return ok('Clear logist success.');
    }

    return fail('InvalidWorkerRole', 'Unsupported worker role.');
  }

  if (!Array.isArray(profile.knownContacts) || !profile.knownContacts.includes(contactId)) {
    return fail('ContactNotKnown', 'Contact not unlocked.');
  }

  const contactDef = businessDefs && businessDefs.staffContactById && businessDefs.staffContactById.get(contactId);
  if (contactDef && contactDef.roleId && contactDef.roleId !== roleId) {
    return fail('ContactRoleMismatch', 'Contact does not match selected role.');
  }

  if (roleId === 'cashier') {
    business.hiredCashierContactId = contactId;
  } else if (roleId === 'merchandiser') {
    business.hiredMerchContactId = contactId;
  } else if (roleId === 'logist') {
    business.hiredLogistContactId = contactId;
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

  const required = Array.isArray(typeDef.requiredModules) ? typeDef.requiredModules : [];
  const installed = Array.isArray(business.installedModules) ? business.installedModules : [];
  const missing = required.filter(id => !installed.includes(id));
  if (missing.length > 0) {
    return fail('MissingRequiredModules', `Missing required modules: ${missing.join(', ')}`);
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

  const installed = Array.isArray(business.installedModules) ? business.installedModules : [];
  if (!installed.includes('storage')) {
    return fail('StorageNotInstalled', 'Storage module not installed.');
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

  const installed = Array.isArray(business.installedModules) ? business.installedModules : [];
  if (!installed.includes('shelves')) {
    return fail('ShelvesNotInstalled', 'Shelves module not installed.');
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

function resetBusinesses(profile) {
  profile.businesses = [];
  return ok('Businesses reset.');
}

module.exports = {
  rentBusiness,
  assignBusinessType,
  installBusinessModule,
  assignSupplier,
  hireBusinessWorker,
  openBusiness,
  closeBusiness,
  setBusinessMarkup,
  unlockContact,
  addBusinessStock,
  addBusinessShelfStock,
  clearBusinessStock,
  resetBusinesses,
};
