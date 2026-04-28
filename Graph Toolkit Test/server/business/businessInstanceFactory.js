function cloneTemplate(template) {
  return JSON.parse(JSON.stringify(template && typeof template === 'object' ? template : {}));
}

function getTypeDefaults(businessTypeId, businessDefs) {
  const typeId = typeof businessTypeId === 'string' ? businessTypeId.trim() : '';
  const type = typeId && businessDefs?.businessTypeById ? businessDefs.businessTypeById.get(typeId) : null;
  const instanceTemplate = cloneTemplate(type?.instanceTemplate);

  return {
    businessTypeId: typeId,
    ...instanceTemplate
  };
}

function createBusinessInstance(template, lotId, rentPerDay, businessTypeId, businessDefs) {
  const instanceId = `biz_${Date.now()}_${Math.floor(Math.random() * 10000)}`;
  const source = cloneTemplate(template);
  const hasBusinessType = typeof businessTypeId === 'string' && businessTypeId.trim().length > 0;
  const typeDefaults = hasBusinessType ? getTypeDefaults(businessTypeId, businessDefs) : {};

  return {
    ...source,
    ...typeDefaults,
    instanceId,
    lotId,
    isOpen: false,
    businessTypeId: hasBusinessType ? businessTypeId.trim() : '',
    rentPerDay: Number.isFinite(rentPerDay) && rentPerDay >= 0 ? rentPerDay : 0
  };
}

function applyBusinessTypeTemplate(business, template, businessTypeId, businessDefs) {
  if (!business || typeof business !== 'object') {
    return business;
  }

  const source = cloneTemplate(template);
  const typeDefaults = getTypeDefaults(businessTypeId, businessDefs);
  const preserved = {
    instanceId: business.instanceId,
    lotId: business.lotId,
    rentPerDay: business.rentPerDay
  };

  Object.assign(business, source, typeDefaults, preserved);
  business.isOpen = false;
  return business;
}

module.exports = {
  createBusinessInstance,
  applyBusinessTypeTemplate
};
