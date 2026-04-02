const fs = require('fs');
const path = require('path');

const DATA_DIR = path.join(__dirname, '..', 'data');

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

function validateLots(lotsData, businessDefs) {
  let errors = 0;
  let warnings = 0;

  if (!lotsData || !Array.isArray(lotsData.lots)) {
    console.error('[server][lots] lots.json missing "lots" array');
    errors++;
  } else {
    const ids = new Set();
    lotsData.lots.forEach((lot, i) => {
      if (!lot || !lot.id || !String(lot.id).trim()) {
        console.error(`[server][lots] lot at index ${i} missing id`);
        errors++;
        return;
      }
      if (ids.has(lot.id)) {
        console.error(`[server][lots] duplicate lot id: ${lot.id}`);
        errors++;
      }
      ids.add(lot.id);
      if (Number.isFinite(lot.rentPerDay) && lot.rentPerDay < 0) {
        console.error(`[server][lots] lot ${lot.id} rentPerDay < 0`);
        errors++;
      }
      if (!Array.isArray(lot.allowedBusinessTypes) || lot.allowedBusinessTypes.length === 0) {
        console.error(`[server][lots] lot ${lot.id} allowedBusinessTypes missing or empty`);
        errors++;
      } else if (businessDefs && businessDefs.businessTypeById) {
        lot.allowedBusinessTypes.forEach(typeId => {
          if (!typeId || !String(typeId).trim()) {
            console.error(`[server][lots] lot ${lot.id} has empty businessType reference`);
            errors++;
          } else if (!businessDefs.businessTypeById.has(typeId)) {
            console.error(`[server][lots] lot ${lot.id} references unknown businessTypeId ${typeId}`);
            errors++;
          }
        });
      }
      if (Number.isFinite(lot.size) && lot.size < 0) {
        console.warn(`[server][lots] lot ${lot.id} size < 0`);
        warnings++;
      }
    });
  }

  if (errors > 0) {
    throw new Error(`[server][lots] validation failed with ${errors} error(s), ${warnings} warning(s)`);
  }
  console.log(`[server][lots] validated with ${warnings} warning(s)`);
}

function loadLotDefinitions(businessDefs) {
  const lotsData = readJson(path.join(DATA_DIR, 'lots.json'));
  validateLots(lotsData, businessDefs);

  const lotById = new Map();
  (lotsData.lots || []).forEach(lot => {
    if (lot && lot.id && !lotById.has(lot.id)) {
      lotById.set(lot.id, lot);
    }
  });

  return {
    lots: lotsData,
    lotById
  };
}

module.exports = {
  loadLotDefinitions
};
