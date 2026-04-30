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

function readJsonFolder(dirPath) {
  if (!fs.existsSync(dirPath)) {
    return [];
  }

  return fs.readdirSync(dirPath)
    .filter(name => name.toLowerCase().endsWith('.json'))
    .sort((a, b) => a.localeCompare(b))
    .map(name => readJson(path.join(dirPath, name)));
}

function buildFromPeople(people) {
  const peopleData = { people: [] };
  const suppliers = { suppliers: [] };
  const staffRoles = { roles: [] };
  const staffContacts = { contacts: [] };

  (Array.isArray(people) ? people : []).forEach(person => {
    if (!person || !person.contactId) {
      return;
    }

    const contactId = String(person.contactId).trim();
    const displayName = person.displayName || contactId;
    const normalizedPerson = {
      contactId,
      displayName,
      salaryPerDay: Number.isFinite(person.salaryPerDay) ? person.salaryPerDay : 0,
      throughputPerHour: Number.isFinite(person.throughputPerHour) ? person.throughputPerHour : 0
    };

    if (person.supplierConfig) {
      normalizedPerson.supplierConfig = {
        productType: person.supplierConfig.productType || '',
        unitBuyPrice: Number.isFinite(person.supplierConfig.unitBuyPrice) ? person.supplierConfig.unitBuyPrice : 0,
        minDeliveryAmount: Number.isFinite(person.supplierConfig.minDeliveryAmount) ? person.supplierConfig.minDeliveryAmount : 0,
        maxDeliveryAmount: Number.isFinite(person.supplierConfig.maxDeliveryAmount) ? person.supplierConfig.maxDeliveryAmount : 0
      };

      suppliers.suppliers.push({
        id: contactId,
        displayName,
        productType: normalizedPerson.supplierConfig.productType,
        unitBuyPrice: normalizedPerson.supplierConfig.unitBuyPrice,
        minDeliveryAmount: normalizedPerson.supplierConfig.minDeliveryAmount,
        maxDeliveryAmount: normalizedPerson.supplierConfig.maxDeliveryAmount
      });
    }

    peopleData.people.push(normalizedPerson);
    staffContacts.contacts.push({
      id: contactId,
      displayName,
      salaryPerDay: normalizedPerson.salaryPerDay,
      throughputPerHour: normalizedPerson.throughputPerHour
    });
  });

  return { peopleData, suppliers, staffRoles, staffContacts };
}

function buildTraderData(traders, items) {
  const itemById = new Map();
  const traderItems = { items: [] };
  const tradersData = { traders: [] };

  (Array.isArray(items) ? items : []).forEach(item => {
    if (!item || !item.id || itemById.has(item.id)) {
      return;
    }

    const normalizedItem = {
      id: String(item.id).trim(),
      category: item.category || '',
      name: item.name || String(item.id).trim(),
      description: item.description || '',
      price: Number.isFinite(item.price) ? item.price : 0,
      storageCapacity: Number.isFinite(item.storageCapacity) ? item.storageCapacity : 0,
      cashCapacity: Number.isFinite(item.cashCapacity) ? item.cashCapacity : 0,
      shelfCapacity: Number.isFinite(item.shelfCapacity) ? item.shelfCapacity : 0
    };

    itemById.set(normalizedItem.id, normalizedItem);
    traderItems.items.push(normalizedItem);
  });

  (Array.isArray(traders) ? traders : []).forEach(trader => {
    if (!trader || !trader.id) {
      return;
    }

    if (Array.isArray(trader.items)) {
      trader.items.forEach(item => {
        if (!item || !item.id || itemById.has(item.id)) {
          return;
        }

        const normalizedItem = {
          id: String(item.id).trim(),
          category: item.category || '',
          name: item.name || String(item.id).trim(),
          description: item.description || '',
          price: Number.isFinite(item.price) ? item.price : 0,
          storageCapacity: Number.isFinite(item.storageCapacity) ? item.storageCapacity : 0,
          cashCapacity: Number.isFinite(item.cashCapacity) ? item.cashCapacity : 0,
          shelfCapacity: Number.isFinite(item.shelfCapacity) ? item.shelfCapacity : 0
        };

        itemById.set(normalizedItem.id, normalizedItem);
        traderItems.items.push(normalizedItem);
      });
    }

    const normalizedTrader = {
      id: String(trader.id).trim(),
      name: trader.name || String(trader.id).trim(),
      itemIds: []
    };

    const sourceItemIds = Array.isArray(trader.itemIds)
      ? trader.itemIds
      : Array.isArray(trader.items)
        ? trader.items.map(item => item && item.id).filter(Boolean)
        : [];

    sourceItemIds.forEach(itemId => {
      if (typeof itemId !== 'string') {
        return;
      }

      const trimmed = itemId.trim();
      if (trimmed.length > 0) {
        normalizedTrader.itemIds.push(trimmed);
      }
    });

    tradersData.traders.push(normalizedTrader);
  });

  return { tradersData, traderItems };
}

function validateBusinessDefinitions(
  businessTypes,
  suppliers,
  staffRoles,
  staffContacts,
  peopleData,
  traders,
  traderItems,
  pizzeriaDemand) {
  let errors = 0;
  let warnings = 0;

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
    });
  }

  if (!peopleData || !Array.isArray(peopleData.people)) {
    console.error('[server][business] people missing "people" array');
    errors++;
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
      if ((Number.isFinite(c.salaryPerDay) && c.salaryPerDay < 0) ||
          (Number.isFinite(c.throughputPerHour) && c.throughputPerHour < 0)) {
        console.warn(`[server][business] staff contact ${c.id} has negative values`);
        warnings++;
      }
    });
  }

  const itemIds = new Set();
  if (!traderItems || !Array.isArray(traderItems.items)) {
    console.error('[server][business] items missing "items" array');
    errors++;
  } else {
    traderItems.items.forEach((item, i) => {
      if (!item || !item.id || !String(item.id).trim()) {
        console.error(`[server][business] item at index ${i} missing id`);
        errors++;
        return;
      }

      if (itemIds.has(item.id)) {
        console.error(`[server][business] duplicate trader item id: ${item.id}`);
        errors++;
      }
      itemIds.add(item.id);

      if (Number.isFinite(item.price) && item.price < 0) {
        console.warn(`[server][business] trader item ${item.id} price < 0`);
        warnings++;
      }
    });
  }

  if (!traders || !Array.isArray(traders.traders)) {
    console.error('[server][business] traders missing "traders" array');
    errors++;
  } else {
    const traderIds = new Set();
    traders.traders.forEach((trader, traderIndex) => {
      if (!trader || !trader.id || !String(trader.id).trim()) {
        console.error(`[server][business] trader at index ${traderIndex} missing id`);
        errors++;
        return;
      }

      if (traderIds.has(trader.id)) {
        console.error(`[server][business] duplicate trader id: ${trader.id}`);
        errors++;
      }
      traderIds.add(trader.id);

      if (!Array.isArray(trader.itemIds)) {
        console.error(`[server][business] trader ${trader.id} missing "itemIds" array`);
        errors++;
        return;
      }

      trader.itemIds.forEach((itemId, itemIndex) => {
        if (!itemId || !String(itemId).trim()) {
          console.error(`[server][business] trader ${trader.id} itemId at index ${itemIndex} missing id`);
          errors++;
          return;
        }

        if (!itemIds.has(itemId)) {
          console.error(`[server][business] trader ${trader.id} references unknown item id: ${itemId}`);
          errors++;
        }
      });
    });
  }

  if (!pizzeriaDemand || !Array.isArray(pizzeriaDemand.ranges)) {
    console.error('[server][business] pizzeria_demand missing "ranges" array');
    errors++;
  } else {
    pizzeriaDemand.ranges.forEach((range, i) => {
      if (!range ||
        !Number.isFinite(range.minPrice) ||
        !Number.isFinite(range.maxPrice) ||
        !Number.isFinite(range.dailyDemand)) {
        console.error(`[server][business] pizzeria_demand range at index ${i} invalid`);
        errors++;
        return;
      }

      if (range.minPrice > range.maxPrice) {
        console.error(`[server][business] pizzeria_demand range at index ${i} has minPrice > maxPrice`);
        errors++;
      }

      if (range.dailyDemand < 0) {
        console.warn(`[server][business] pizzeria_demand range at index ${i} has negative dailyDemand`);
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
  const businessInstanceTemplate = readJson(path.join(BUSINESS_DIR, 'business_instance_template.json'));
  const peopleRows = readJsonFolder(path.join(BUSINESS_DIR, 'People'));
  const itemRows = readJsonFolder(path.join(BUSINESS_DIR, 'Items'));
  const traderRows = readJsonFolder(path.join(BUSINESS_DIR, 'Traders'));

  const legacyPeople = peopleRows.length === 0 ? readJson(path.join(BUSINESS_DIR, 'people.json')) : null;
  const legacyTraders = traderRows.length === 0 ? readJson(path.join(BUSINESS_DIR, 'traders.json')) : null;

  const { peopleData, suppliers, staffRoles, staffContacts } = buildFromPeople(
    peopleRows.length > 0 ? peopleRows : legacyPeople?.people
  );
  const { tradersData, traderItems } = buildTraderData(
    traderRows.length > 0 ? traderRows : legacyTraders?.traders,
    itemRows.length > 0 ? itemRows : []
  );
  const pizzeriaDemand = readJson(path.join(BUSINESS_DIR, 'pizzeria_demand.json'));

  validateBusinessDefinitions(
    businessTypes,
    suppliers,
    staffRoles,
    staffContacts,
    peopleData,
    tradersData,
    traderItems,
    pizzeriaDemand);

  const businessTypeById = new Map();
  (businessTypes.businessTypes || []).forEach(item => {
    if (item && item.id && !businessTypeById.has(item.id)) businessTypeById.set(item.id, item);
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

  const traderItemById = new Map();
  (traderItems.items || []).forEach(item => {
    if (item && item.id && !traderItemById.has(item.id)) {
      traderItemById.set(item.id, item);
    }
  });

  const traderById = new Map();
  (tradersData.traders || []).forEach(trader => {
    if (trader && trader.id && !traderById.has(trader.id)) {
      traderById.set(trader.id, trader);
    }
  });

  const demandByBusinessTypeId = new Map();
  const pizzeriaRanges = Array.isArray(pizzeriaDemand.ranges)
    ? pizzeriaDemand.ranges
      .map(range => ({
        minPrice: range.minPrice,
        maxPrice: range.maxPrice,
        dailyDemand: range.dailyDemand
      }))
      .sort((a, b) => a.minPrice - b.minPrice)
    : [];
  demandByBusinessTypeId.set('grocery_store', pizzeriaRanges);
  demandByBusinessTypeId.set('pizza_shop', pizzeriaRanges);

  return {
    businessTypes,
    businessInstanceTemplate,
    peopleData,
    suppliers,
    staffRoles,
    staffContacts,
    traders: tradersData,
    traderItems,
    businessTypeById,
    supplierById,
    staffRoleById,
    staffContactById,
    traderById,
    traderItemById,
    demandByBusinessTypeId
  };
}

module.exports = {
  loadBusinessDefinitions
};
