const fs = require('fs');
const path = require('path');

const BUSINESS_DIR = path.join(__dirname, '..', 'data', 'business');

function readJson(filePath) {
  if (!fs.existsSync(filePath)) {
    throw new Error(`File not found: ${filePath}`);
  }
  const raw = fs.readFileSync(filePath, 'utf8');
  if (!raw || !raw.trim()) {
    throw new Error(`Empty JSON: ${filePath}`);
  }
  return JSON.parse(raw);
}

function validateBusinessDefinitions(businessTypes, modules, suppliers, staffRoles, staffContacts, behaviors) {
  let errors = 0;
  let warnings = 0;

  const moduleIds = new Set();
  if (!modules || !Array.isArray(modules.modules)) {
    console.error('[server][business] modules missing "modules" array');
    errors++;
  } else {
    modules.modules.forEach((m, i) => {
      if (!m || !m.id || !String(m.id).trim()) {
        console.error(`[server][business] module at index ${i} missing id`);
        errors++;
        return;
      }
      if (moduleIds.has(m.id)) {
        console.error(`[server][business] duplicate module id: ${m.id}`);
        errors++;
      }
      moduleIds.add(m.id);
      if (Number.isFinite(m.installCost) && m.installCost < 0) {
        console.warn(`[server][business] module ${m.id} installCost < 0`);
        warnings++;
      }
    });
  }

  const businessTypeIds = new Set();
  if (!businessTypes || !Array.isArray(businessTypes.businessTypes)) {
    console.error('[server][business] business_types missing "businessTypes" array');
    errors++;
  } else {
    businessTypes.businessTypes.forEach((b, i) => {
      if (!b || !b.id || !String(b.id).trim()) {
        console.error(`[server][business] business type at index ${i} missing id`);
        errors++;
        return;
      }
      if (businessTypeIds.has(b.id)) {
        console.error(`[server][business] duplicate business type id: ${b.id}`);
        errors++;
      }
      businessTypeIds.add(b.id);
      if (!Array.isArray(b.requiredModules) || b.requiredModules.length === 0) {
        console.error(`[server][business] business type ${b.id} requiredModules missing or empty`);
        errors++;
      } else {
        b.requiredModules.forEach(moduleId => {
          if (!moduleId || !String(moduleId).trim()) {
            console.error(`[server][business] business type ${b.id} has empty module reference`);
            errors++;
          } else if (!moduleIds.has(moduleId)) {
            console.error(`[server][business] business type ${b.id} references unknown module ${moduleId}`);
            errors++;
          }
        });
      }
      if (Number.isFinite(b.defaultStorageCapacity) && b.defaultStorageCapacity < 0) {
        console.warn(`[server][business] business type ${b.id} defaultStorageCapacity < 0`);
        warnings++;
      }
      if (Number.isFinite(b.defaultShelfCapacity) && b.defaultShelfCapacity < 0) {
        console.warn(`[server][business] business type ${b.id} defaultShelfCapacity < 0`);
        warnings++;
      }
    });
  }

  if (!suppliers || !Array.isArray(suppliers.suppliers)) {
    console.error('[server][business] suppliers missing "suppliers" array');
    errors++;
  } else {
    const ids = new Set();
    suppliers.suppliers.forEach((s, i) => {
      if (!s || !s.id || !String(s.id).trim()) {
        console.error(`[server][business] supplier at index ${i} missing id`);
        errors++;
        return;
      }
      if (ids.has(s.id)) {
        console.error(`[server][business] duplicate supplier id: ${s.id}`);
        errors++;
      }
      ids.add(s.id);
      if ((Number.isFinite(s.unitBuyPrice) && s.unitBuyPrice < 0) ||
          (Number.isFinite(s.minDeliveryAmount) && s.minDeliveryAmount < 0) ||
          (Number.isFinite(s.maxDeliveryAmount) && s.maxDeliveryAmount < 0)) {
        console.warn(`[server][business] supplier ${s.id} has negative values`);
        warnings++;
      }
    });
  }

  if (!staffRoles || !Array.isArray(staffRoles.roles)) {
    console.error('[server][business] staff_roles missing "roles" array');
    errors++;
  } else {
    const ids = new Set();
    staffRoles.roles.forEach((r, i) => {
      if (!r || !r.id || !String(r.id).trim()) {
        console.error(`[server][business] staff role at index ${i} missing id`);
        errors++;
        return;
      }
      if (ids.has(r.id)) {
        console.error(`[server][business] duplicate staff role id: ${r.id}`);
        errors++;
      }
      ids.add(r.id);
      if ((Number.isFinite(r.salaryPerDay) && r.salaryPerDay < 0) ||
          (Number.isFinite(r.throughputPerHour) && r.throughputPerHour < 0)) {
        console.warn(`[server][business] staff role ${r.id} has negative values`);
        warnings++;
      }
    });
  }

  const roleIds = new Set();
  if (staffRoles && Array.isArray(staffRoles.roles)) {
    staffRoles.roles.forEach(r => {
      if (r && r.id && String(r.id).trim()) {
        roleIds.add(String(r.id).trim());
      }
    });
  }

  if (!staffContacts || !Array.isArray(staffContacts.contacts)) {
    console.error('[server][business] staff_contacts missing "contacts" array');
    errors++;
  } else {
    const ids = new Set();
    staffContacts.contacts.forEach((c, i) => {
      if (!c || !c.id || !String(c.id).trim()) {
        console.error(`[server][business] staff contact at index ${i} missing id`);
        errors++;
        return;
      }

      if (ids.has(c.id)) {
        console.error(`[server][business] duplicate staff contact id: ${c.id}`);
        errors++;
      }
      ids.add(c.id);

      if (!c.roleId || !String(c.roleId).trim()) {
        console.error(`[server][business] staff contact ${c.id} missing roleId`);
        errors++;
      } else if (!roleIds.has(c.roleId)) {
        console.error(`[server][business] staff contact ${c.id} references unknown roleId: ${c.roleId}`);
        errors++;
      }

      if ((Number.isFinite(c.salaryPerDay) && c.salaryPerDay < 0) ||
          (Number.isFinite(c.throughputPerHour) && c.throughputPerHour < 0)) {
        console.warn(`[server][business] staff contact ${c.id} has negative values`);
        warnings++;
      }
    });
  }

  if (!behaviors || !Array.isArray(behaviors.behaviors)) {
    console.error('[server][business] customer_behavior missing "behaviors" array');
    errors++;
  } else {
    const ids = new Set();
    behaviors.behaviors.forEach((b, i) => {
      if (!b || !b.businessTypeId || !String(b.businessTypeId).trim()) {
        console.error(`[server][business] customer behavior at index ${i} missing businessTypeId`);
        errors++;
        return;
      }
      if (ids.has(b.businessTypeId)) {
        console.error(`[server][business] duplicate customer behavior for ${b.businessTypeId}`);
        errors++;
      }
      ids.add(b.businessTypeId);
      if (!businessTypeIds.has(b.businessTypeId)) {
        console.error(`[server][business] customer behavior references unknown businessTypeId ${b.businessTypeId}`);
        errors++;
      }
      if (Number.isFinite(b.arrivalRatePerHour) && b.arrivalRatePerHour < 0) {
        console.warn(`[server][business] customer behavior ${b.businessTypeId} arrivalRatePerHour < 0`);
        warnings++;
      }
    });
  }

  if (errors > 0) {
    throw new Error(`[server][business] validation failed with ${errors} error(s), ${warnings} warning(s)`);
  }
  console.log(`[server][business] validated with ${warnings} warning(s)`);
}

function loadBusinessDefinitions() {
  fs.mkdirSync(BUSINESS_DIR, { recursive: true });

  const businessTypes = readJson(path.join(BUSINESS_DIR, 'business_types.json'));
  const modules = readJson(path.join(BUSINESS_DIR, 'business_modules.json'));
  const suppliers = readJson(path.join(BUSINESS_DIR, 'suppliers.json'));
  const staffRoles = readJson(path.join(BUSINESS_DIR, 'staff_roles.json'));
  const staffContacts = readJson(path.join(BUSINESS_DIR, 'staff_contacts.json'));
  const behaviors = readJson(path.join(BUSINESS_DIR, 'customer_behavior.json'));

  validateBusinessDefinitions(businessTypes, modules, suppliers, staffRoles, staffContacts, behaviors);

  const businessTypeById = new Map();
  (businessTypes.businessTypes || []).forEach(item => {
    if (item && item.id && !businessTypeById.has(item.id)) businessTypeById.set(item.id, item);
  });

  const moduleById = new Map();
  (modules.modules || []).forEach(item => {
    if (item && item.id && !moduleById.has(item.id)) moduleById.set(item.id, item);
  });

  const supplierById = new Map();
  (suppliers.suppliers || []).forEach(item => {
    if (item && item.id && !supplierById.has(item.id)) supplierById.set(item.id, item);
  });

  const staffRoleById = new Map();
  (staffRoles.roles || []).forEach(item => {
    if (item && item.id && !staffRoleById.has(item.id)) staffRoleById.set(item.id, item);
  });

  const staffContactById = new Map();
  (staffContacts.contacts || []).forEach(item => {
    if (item && item.id && !staffContactById.has(item.id)) staffContactById.set(item.id, item);
  });

  const behaviorByBusinessTypeId = new Map();
  (behaviors.behaviors || []).forEach(item => {
    if (item && item.businessTypeId && !behaviorByBusinessTypeId.has(item.businessTypeId)) {
      behaviorByBusinessTypeId.set(item.businessTypeId, item);
    }
  });

  return {
    businessTypes,
    modules,
    suppliers,
    staffRoles,
    staffContacts,
    behaviors,
    businessTypeById,
    moduleById,
    supplierById,
    staffRoleById,
    staffContactById,
    behaviorByBusinessTypeId
  };
}

module.exports = {
  loadBusinessDefinitions
};
