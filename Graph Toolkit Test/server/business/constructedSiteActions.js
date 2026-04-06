const { findConstructedSiteBySiteId } = require('./constructedSiteState');

function ok(message) {
  return { ok: true, message: message || '' };
}

function fail(errorCode, message) {
  return { ok: false, errorCode, message: message || '' };
}

function constructSiteVisual(profile, data) {
  const siteId = data && data.siteId;
  const visualId = data && data.visualId;

  if (!siteId || !String(siteId).trim()) {
    return fail('SiteIdEmpty', 'siteId is required.');
  }

  if (!visualId || !String(visualId).trim()) {
    return fail('VisualIdEmpty', 'visualId is required.');
  }

  const normalizedSiteId = String(siteId).trim();
  const normalizedVisualId = String(visualId).trim();
  profile.constructedSites = Array.isArray(profile.constructedSites) ? profile.constructedSites : [];

  const existing = findConstructedSiteBySiteId(profile, normalizedSiteId);
  if (existing) {
    existing.visualId = normalizedVisualId;
    existing.isConstructed = true;
  } else {
    profile.constructedSites.push({
      siteId: normalizedSiteId,
      visualId: normalizedVisualId,
      isConstructed: true
    });
  }

  return ok('Construct site visual success.');
}

function removeSiteVisual(profile, data) {
  const siteId = data && data.siteId;
  if (!siteId || !String(siteId).trim()) {
    return fail('SiteIdEmpty', 'siteId is required.');
  }

  const normalizedSiteId = String(siteId).trim();
  profile.constructedSites = Array.isArray(profile.constructedSites) ? profile.constructedSites : [];
  profile.constructedSites = profile.constructedSites.filter(site => site && site.siteId !== normalizedSiteId);
  return ok('Remove site visual success.');
}

module.exports = {
  constructSiteVisual,
  removeSiteVisual
};
