function normalizeConstructedSite(site) {
  const normalized = {
    siteId: typeof site?.siteId === 'string' ? site.siteId.trim() : '',
    visualId: typeof site?.visualId === 'string' ? site.visualId.trim() : '',
    isConstructed: Boolean(site?.isConstructed)
  };

  if (!normalized.siteId || !normalized.visualId) {
    normalized.isConstructed = false;
  }

  return normalized;
}

function normalizeConstructedSites(profile) {
  if (!profile) return profile;

  if (!Array.isArray(profile.constructedSites)) {
    profile.constructedSites = [];
  }

  profile.constructedSites = profile.constructedSites
    .map(normalizeConstructedSite)
    .filter(site => site.siteId && site.visualId && site.isConstructed);

  return profile;
}

function sanitizeConstructedSites(profile) {
  if (!profile) return profile;

  const seenSiteIds = new Set();
  const sanitized = [];

  for (const site of profile.constructedSites || []) {
    if (!site || !site.siteId || !site.visualId || !site.isConstructed) {
      continue;
    }

    if (seenSiteIds.has(site.siteId)) {
      continue;
    }

    seenSiteIds.add(site.siteId);
    sanitized.push({
      siteId: site.siteId,
      visualId: site.visualId,
      isConstructed: true
    });
  }

  profile.constructedSites = sanitized;
  return profile;
}

function findConstructedSiteBySiteId(profile, siteId) {
  if (!profile || !Array.isArray(profile.constructedSites) || !siteId) {
    return null;
  }

  return profile.constructedSites.find(site => site.siteId === siteId) || null;
}

module.exports = {
  normalizeConstructedSites,
  sanitizeConstructedSites,
  findConstructedSiteBySiteId
};
