const http = require('http');
const fs = require('fs');
const path = require('path');
const { loadBusinessDefinitions } = require('./business/businessDefinitions');
const { loadLotDefinitions } = require('./business/lotDefinitions');
const { normalizeBusinessProfile, sanitizeBusinessProfile } = require('./business/businessState');
const { normalizeConstructedSites, sanitizeConstructedSites } = require('./business/constructedSiteState');
const businessActions = require('./business/businessActions');
const constructedSiteActions = require('./business/constructedSiteActions');

const PORT = process.env.PORT ? Number(process.env.PORT) : 3000;
const HOST = process.env.HOST || '127.0.0.1';
const DATA_DIR = path.join(__dirname, 'data');
const PLAYER_DIR = path.join(__dirname, 'playerData');

fs.mkdirSync(DATA_DIR, { recursive: true });
fs.mkdirSync(PLAYER_DIR, { recursive: true });

process.on('uncaughtException', err => {
  console.error('[server] UncaughtException:', err && err.stack ? err.stack : err);
});

process.on('unhandledRejection', err => {
  console.error('[server] UnhandledRejection:', err && err.stack ? err.stack : err);
});

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

function writeJson(filePath, data) {
  fs.writeFileSync(filePath, JSON.stringify(data, null, 2), 'utf8');
}

function safePlayerId(value) {
  if (!value || typeof value !== 'string') return 'player';
  const cleaned = value.replace(/[^a-zA-Z0-9_-]/g, '');
  return cleaned.length > 0 ? cleaned : 'player';
}

function loadDefinitions() {
  const quests = readJson(path.join(DATA_DIR, 'quests.json'));
  const buildings = readJson(path.join(DATA_DIR, 'buildings.json'));
  const economy = readJson(path.join(DATA_DIR, 'economy.json'));
  const tradeConfig = readJson(path.join(DATA_DIR, 'tradeConfig.json'));

  validateDefinitions(quests, buildings, economy, tradeConfig);

  const questById = new Map();
  (quests.quests || []).forEach(q => {
    if (q && q.id && !questById.has(q.id)) questById.set(q.id, q);
  });

  const buildingById = new Map();
  (buildings.buildings || []).forEach(b => {
    if (b && b.id && !buildingById.has(b.id)) buildingById.set(b.id, b);
  });

  return { questById, buildingById, economy, tradeConfig };
}

function validateDefinitions(quests, buildings, economy, tradeConfig) {
  let errors = 0;
  let warnings = 0;

  if (!quests || !Array.isArray(quests.quests)) {
    console.error('[server][validate] quests.json missing "quests" array');
    errors++;
  } else {
    const ids = new Set();
    quests.quests.forEach((q, i) => {
      if (!q || !q.id || typeof q.id !== 'string' || !q.id.trim()) {
        console.error(`[server][validate] quest at index ${i} missing id`);
        errors++;
        return;
      }
      if (ids.has(q.id)) {
        console.error(`[server][validate] duplicate quest id: ${q.id}`);
        errors++;
      }
      ids.add(q.id);
      if (Number.isFinite(q.rewardMoney) && q.rewardMoney < 0) {
        console.warn(`[server][validate] quest ${q.id} rewardMoney < 0`);
        warnings++;
      }
    });
  }

  if (!buildings || !Array.isArray(buildings.buildings)) {
    console.error('[server][validate] buildings.json missing "buildings" array');
    errors++;
  } else {
    const ids = new Set();
    buildings.buildings.forEach((b, i) => {
      if (!b || !b.id || typeof b.id !== 'string' || !b.id.trim()) {
        console.error(`[server][validate] building at index ${i} missing id`);
        errors++;
        return;
      }
      if (ids.has(b.id)) {
        console.error(`[server][validate] duplicate building id: ${b.id}`);
        errors++;
      }
      ids.add(b.id);
      if (Number.isFinite(b.purchaseCost) && b.purchaseCost < 0) {
        console.warn(`[server][validate] building ${b.id} purchaseCost < 0`);
        warnings++;
      }
      if (!b.siteId || typeof b.siteId !== 'string' || !b.siteId.trim()) {
        console.warn(`[server][validate] building ${b.id} missing siteId`);
        warnings++;
      }
      if (!b.visualId || typeof b.visualId !== 'string' || !b.visualId.trim()) {
        console.warn(`[server][validate] building ${b.id} missing visualId`);
        warnings++;
      }
    });
  }

  if (!economy || typeof economy !== 'object') {
    console.error('[server][validate] economy.json missing or invalid');
    errors++;
  } else if (Number.isFinite(economy.startMoney) && economy.startMoney < 0) {
    console.warn('[server][validate] economy.startMoney < 0');
    warnings++;
  }

  if (!tradeConfig || typeof tradeConfig !== 'object') {
    console.error('[server][validate] tradeConfig.json missing or invalid');
    errors++;
  } else if (!Array.isArray(tradeConfig.ranges) || tradeConfig.ranges.length === 0) {
    console.error('[server][validate] tradeConfig.json missing "ranges" array');
    errors++;
  }

  console.log(`[server][validate] completed with ${errors} error(s), ${warnings} warning(s)`);
}

const defs = (() => {
  try {
    return loadDefinitions();
  } catch (err) {
    console.error('[server] Failed to load definitions:', err.message);
    process.exit(1);
  }
})();

const businessDefs = (() => {
  try {
    return loadBusinessDefinitions();
  } catch (err) {
    console.error('[server] Failed to load business definitions:', err.message);
    process.exit(1);
  }
})();

const lotDefs = (() => {
  try {
    return loadLotDefinitions(businessDefs);
  } catch (err) {
    console.error('[server] Failed to load lot definitions:', err.message);
    process.exit(1);
  }
})();

function ensureBuildingStates(profile) {
  if (!profile || !defs || !defs.buildingById) return;

  const existing = new Map();
  (profile.buildingStates || []).forEach(state => {
    if (state && state.id) existing.set(state.id, state);
  });

  profile.buildingStates = [];
  const ownedIds = profile.ownedBuildings || [];
  ownedIds.forEach(id => {
    const def = defs.buildingById.get(id);
    if (!def) return;
    const state = existing.get(id);
    const level = state && Number.isFinite(state.level) ? state.level : 0;
    const currentIncome = state && Number.isFinite(state.currentIncome) ? state.currentIncome : 0;
    const currentExpenses = state && Number.isFinite(state.currentExpenses) ? state.currentExpenses : 0;

    profile.buildingStates.push({
      id,
      owned: true,
      level,
      currentIncome,
      currentExpenses
    });
  });
}

function loadPlayerProfile(playerId) {
  const id = safePlayerId(playerId);
  const filePath = path.join(PLAYER_DIR, `${id}.json`);

  if (!fs.existsSync(filePath)) {
    const profile = {
      playerId: id,
      money: defs.economy && Number.isFinite(defs.economy.startMoney) ? defs.economy.startMoney : 0,
      bargaining: defs.economy && Number.isFinite(defs.economy.baseBargaining) ? defs.economy.baseBargaining : 0,
      speech: defs.economy && Number.isFinite(defs.economy.baseSpeech) ? defs.economy.baseSpeech : 0,
      trading: defs.economy && Number.isFinite(defs.economy.baseTrading) ? defs.economy.baseTrading : 0,
      speed: defs.economy && Number.isFinite(defs.economy.baseSpeed) ? defs.economy.baseSpeed : 0,
      damage: defs.economy && Number.isFinite(defs.economy.baseDamage) ? defs.economy.baseDamage : 0,
      health: defs.economy && Number.isFinite(defs.economy.baseHealth) ? defs.economy.baseHealth : 0,
      activeQuests: [],
      completedQuests: [],
      ownedBuildings: [],
      buildingStates: [],
      graphCheckpoints: {},
      constructedSites: [],
      businesses: [],
      knownContacts: []
    };
    ensureBuildingStates(profile);
    normalizeBusinessProfile(profile);
    sanitizeBusinessProfile(profile, businessDefs);
    normalizeConstructedSites(profile);
    sanitizeConstructedSites(profile);
    writeJson(filePath, profile);
    return profile;
  }

  const profile = readJson(filePath);
  if (!profile.activeQuests) profile.activeQuests = [];
  if (!profile.completedQuests) profile.completedQuests = [];
  if (!profile.ownedBuildings) profile.ownedBuildings = [];
  if (!profile.buildingStates) profile.buildingStates = [];
  if (!profile.graphCheckpoints || typeof profile.graphCheckpoints !== 'object') profile.graphCheckpoints = {};
  if (!profile.constructedSites) profile.constructedSites = [];
  if (!profile.businesses) profile.businesses = [];
  if (!profile.knownContacts) profile.knownContacts = [];
  if (!Number.isFinite(profile.bargaining)) profile.bargaining = defs.economy?.baseBargaining ?? 0;
  if (!Number.isFinite(profile.speech)) profile.speech = defs.economy?.baseSpeech ?? 0;
  if (!Number.isFinite(profile.trading)) profile.trading = defs.economy?.baseTrading ?? 0;
  if (!Number.isFinite(profile.speed)) profile.speed = defs.economy?.baseSpeed ?? 0;
  if (!Number.isFinite(profile.damage)) profile.damage = defs.economy?.baseDamage ?? 0;
  if (!Number.isFinite(profile.health)) profile.health = defs.economy?.baseHealth ?? 0;
  ensureBuildingStates(profile);
  normalizeBusinessProfile(profile);
  sanitizeBusinessProfile(profile, businessDefs);
  normalizeConstructedSites(profile);
  sanitizeConstructedSites(profile);
  const ownedCount = (profile.ownedBuildings || []).length;
  if ((profile.buildingStates || []).length !== ownedCount)
  {
    savePlayerProfile(profile);
  }
  return profile;
}

function savePlayerProfile(profile) {
  const id = safePlayerId(profile.playerId);
  const filePath = path.join(PLAYER_DIR, `${id}.json`);
  ensureBuildingStates(profile);
  if (!profile.graphCheckpoints || typeof profile.graphCheckpoints !== 'object') profile.graphCheckpoints = {};
  normalizeBusinessProfile(profile);
  sanitizeBusinessProfile(profile, businessDefs);
  normalizeConstructedSites(profile);
  sanitizeConstructedSites(profile);
  writeJson(filePath, profile);
}

function toGraphCheckpointList(profile) {
  const map = profile && profile.graphCheckpoints ? profile.graphCheckpoints : {};
  const list = [];
  if (!map || typeof map !== 'object') return list;
  Object.keys(map).forEach(graphId => {
    const checkpointId = map[graphId];
    if (!graphId || !checkpointId) return;
    list.push({ graphId, checkpointId });
  });
  return list;
}

function toResponseProfile(profile) {
  return {
    money: profile.money,
    bargaining: profile.bargaining,
    speech: profile.speech,
    trading: profile.trading,
    speed: profile.speed,
    damage: profile.damage,
    health: profile.health,
    activeQuests: profile.activeQuests || [],
    completedQuests: profile.completedQuests || [],
    buildings: profile.ownedBuildings || [],
    buildingStates: profile.buildingStates || [],
    graphCheckpoints: toGraphCheckpointList(profile),
    constructedSites: profile.constructedSites || [],
    businesses: profile.businesses || [],
    knownContacts: profile.knownContacts || []
  };
}

function respondJson(res, status, body) {
  const json = JSON.stringify(body);
  res.writeHead(status, { 'Content-Type': 'application/json' });
  res.end(json);
}

function fail(res, errorCode, message, profile) {
  respondJson(res, 200, {
    success: false,
    errorCode,
    message: message || '',
    profile: profile ? toResponseProfile(profile) : undefined
  });
}

function success(res, message, profile) {
  respondJson(res, 200, {
    success: true,
    errorCode: null,
    message: message || '',
    profile: toResponseProfile(profile)
  });
}

function handleBuyBuilding(req, res, payload) {
  const playerId = payload.playerId || 'player';
  const buildingId = payload.data && payload.data.buildingId;
  const questAction = payload.data && payload.data.questAction;
  const questId = payload.data && payload.data.questId;
  console.log(`[server] action=buy_building playerId=${playerId} buildingId=${buildingId}`);
  if (!buildingId) return fail(res, 'BuildingIdEmpty', 'buildingId is required.');

  const building = defs.buildingById.get(buildingId);
  console.log(`[server] buy_building buildingId=${buildingId} def=${building ? building.id : 'null'}`);
  if (!building) return fail(res, 'BuildingNotFound', 'Building not found.');

  const profile = loadPlayerProfile(playerId);

  let quest = null;
  if (questAction) {
    if (!questId) return fail(res, 'QuestIdEmpty', 'questId is required.');
    quest = defs.questById.get(questId);
    if (!quest) return fail(res, 'QuestNotFound', 'Quest not found.');
    if (questAction === 'start') {
      if ((profile.activeQuests || []).includes(questId)) {
        return fail(res, 'QuestAlreadyActive', 'Quest already active.', profile);
      }
      if ((profile.completedQuests || []).includes(questId)) {
        return fail(res, 'QuestAlreadyCompleted', 'Quest already completed.', profile);
      }
    } else if (questAction === 'complete') {
      if ((profile.completedQuests || []).includes(questId)) {
        return fail(res, 'QuestAlreadyCompleted', 'Quest already completed.', profile);
      }
      if (!(profile.activeQuests || []).includes(questId)) {
        return fail(res, 'QuestNotActive', 'Quest is not active.', profile);
      }
    } else {
      return fail(res, 'InvalidQuestAction', `Unknown questAction: ${questAction}`);
    }
  }

  if ((profile.ownedBuildings || []).includes(buildingId)) {
    return fail(res, 'BuildingAlreadyOwned', 'Building already owned.', profile);
  }

  const cost = Number(building.purchaseCost) || 0;
  console.log(`[server] purchaseCost=${cost}`);
  if (profile.money < cost) {
    return fail(res, 'NotEnoughMoney', 'Not enough money.', profile);
  }

  profile.money -= cost;
  profile.ownedBuildings.push(buildingId);
  applyConstructedSiteFromBuilding(profile, building);

  if (questAction === 'start') {
    profile.activeQuests.push(questId);
  } else if (questAction === 'complete') {
    const reward = Number(quest.rewardMoney) || 0;
    profile.money += reward;
    profile.activeQuests = (profile.activeQuests || []).filter(q => q !== questId);
    profile.completedQuests.push(questId);
  }

  ensureBuildingStates(profile);
  savePlayerProfile(profile);
  return success(res, 'Buy building success.', profile);
}

function applyConstructedSiteFromBuilding(profile, building) {
  if (!profile || !building) return;

  const siteId = typeof building.siteId === 'string' ? building.siteId.trim() : '';
  const visualId = typeof building.visualId === 'string' ? building.visualId.trim() : '';
  if (!siteId || !visualId) return;

  normalizeConstructedSites(profile);
  const existing = findConstructedSite(profile, siteId);
  if (existing) {
    existing.visualId = visualId;
    existing.isConstructed = true;
    return;
  }

  profile.constructedSites.push({
    siteId,
    visualId,
    isConstructed: true
  });
}

function findConstructedSite(profile, siteId) {
  if (!profile || !Array.isArray(profile.constructedSites) || !siteId) {
    return null;
  }

  return profile.constructedSites.find(site => site && site.siteId === siteId) || null;
}

function handleStartQuest(req, res, payload) {
  const playerId = payload.playerId || 'player';
  const questId = payload.data && payload.data.questId;
  console.log(`[server] action=start_quest playerId=${playerId} questId=${questId}`);
  if (!questId) return fail(res, 'QuestIdEmpty', 'questId is required.');

  const quest = defs.questById.get(questId);
  if (!quest) return fail(res, 'QuestNotFound', 'Quest not found.');

  const profile = loadPlayerProfile(playerId);

  if ((profile.activeQuests || []).includes(questId)) {
    return fail(res, 'QuestAlreadyActive', 'Quest already active.', profile);
  }

  if ((profile.completedQuests || []).includes(questId)) {
    return fail(res, 'QuestAlreadyCompleted', 'Quest already completed.', profile);
  }

  profile.activeQuests.push(questId);
  savePlayerProfile(profile);
  return success(res, 'Start quest success.', profile);
}

function handleCompleteQuest(req, res, payload) {
  const playerId = payload.playerId || 'player';
  const questId = payload.data && payload.data.questId;
  console.log(`[server] action=complete_quest playerId=${playerId} questId=${questId}`);
  if (!questId) return fail(res, 'QuestIdEmpty', 'questId is required.');

  const quest = defs.questById.get(questId);
  if (!quest) return fail(res, 'QuestNotFound', 'Quest not found.');

  const profile = loadPlayerProfile(playerId);

  if ((profile.completedQuests || []).includes(questId)) {
    return fail(res, 'QuestAlreadyCompleted', 'Quest already completed.', profile);
  }

  if (!(profile.activeQuests || []).includes(questId)) {
    return fail(res, 'QuestNotActive', 'Quest is not active.', profile);
  }

  const reward = Number(quest.rewardMoney) || 0;
  profile.money += reward;
  profile.activeQuests = (profile.activeQuests || []).filter(q => q !== questId);
  profile.completedQuests.push(questId);

  savePlayerProfile(profile);
  return success(res, 'Complete quest success.', profile);
}

function handleFailQuest(req, res, payload) {
  const playerId = payload.playerId || 'player';
  const questId = payload.data && payload.data.questId;
  console.log(`[server] action=fail_quest playerId=${playerId} questId=${questId}`);
  if (!questId) return fail(res, 'QuestIdEmpty', 'questId is required.');

  const quest = defs.questById.get(questId);
  if (!quest) return fail(res, 'QuestNotFound', 'Quest not found.');

  const profile = loadPlayerProfile(playerId);

  if (!(profile.activeQuests || []).includes(questId)) {
    return fail(res, 'QuestNotActive', 'Quest is not active.', profile);
  }

  profile.activeQuests = (profile.activeQuests || []).filter(q => q !== questId);
  savePlayerProfile(profile);
  return success(res, 'Fail quest success.', profile);
}

function handleAddMoney(req, res, payload) {
  const playerId = payload.playerId || 'player';
  const amount = payload.data && Number(payload.data.amount);
  console.log(`[server] action=add_money playerId=${playerId} amount=${amount}`);

  if (!Number.isFinite(amount) || amount <= 0) {
    return fail(res, 'InvalidAmount', 'amount must be > 0.');
  }

  const profile = loadPlayerProfile(playerId);

  profile.money += amount;
  savePlayerProfile(profile);
  return success(res, 'Add money success.', profile);
}

function handleSpendMoney(req, res, payload) {
  const playerId = payload.playerId || 'player';
  const amount = payload.data && Number(payload.data.amount);
  console.log(`[server] action=spend_money playerId=${playerId} amount=${amount}`);

  if (!Number.isFinite(amount) || amount <= 0) {
    return fail(res, 'InvalidAmount', 'amount must be > 0.');
  }

  const profile = loadPlayerProfile(playerId);

  if (profile.money < amount) {
    return fail(res, 'NotEnoughMoney', 'Not enough money.', profile);
  }

  profile.money -= amount;
  savePlayerProfile(profile);
  return success(res, 'Spend money success.', profile);
}

function getBaseTradeChance(percent) {
  const ranges = defs.tradeConfig && Array.isArray(defs.tradeConfig.ranges) ? defs.tradeConfig.ranges : [];
  for (const range of ranges) {
    const min = Number(range.minPercent);
    const max = Number(range.maxPercent);
    const base = Number(range.baseChance);
    if (!Number.isFinite(min) || !Number.isFinite(max) || !Number.isFinite(base)) continue;
    if (percent >= min && percent <= max) return base;
  }
  return 0;
}

function handleSubmitTradeOffer(req, res, payload) {
  const playerId = payload.playerId || 'player';
  const buildingId = payload.data && payload.data.buildingId;
  const offeredAmount = payload.data && Number(payload.data.offeredAmount);
  console.log(`[server] action=submit_trade_offer playerId=${playerId} buildingId=${buildingId} offeredAmount=${offeredAmount}`);

  if (!buildingId) return fail(res, 'BuildingIdEmpty', 'buildingId is required.');
  if (!Number.isFinite(offeredAmount) || offeredAmount < 1) {
    return fail(res, 'InvalidOffer', 'offeredAmount must be >= 1.');
  }

  const building = defs.buildingById.get(buildingId);
  if (!building) return fail(res, 'BuildingNotFound', 'Building not found.');

  const fullPrice = Number(building.purchaseCost) || 0;
  if (offeredAmount > fullPrice) {
    return fail(res, 'OfferTooHigh', 'offeredAmount exceeds full price.');
  }

  const profile = loadPlayerProfile(playerId);

  if ((profile.ownedBuildings || []).includes(buildingId)) {
    return fail(res, 'BuildingAlreadyOwned', 'Building already owned.', profile);
  }

  if (profile.money < offeredAmount) {
    return fail(res, 'NotEnoughMoney', 'Not enough money.', profile);
  }

  const percent = fullPrice > 0 ? (offeredAmount / fullPrice) * 100 : 0;
  const baseChance = getBaseTradeChance(percent);
  const tradingBonus = Number(profile.trading) || 0;
  const maxChance = Number(defs.tradeConfig && defs.tradeConfig.maxFinalChance) || 95;
  let finalChance = baseChance + tradingBonus;
  finalChance = Math.max(0, Math.min(maxChance, finalChance));

  const roll = Math.floor(Math.random() * 100);
  const successRoll = roll < finalChance;

  if (!successRoll) {
    return fail(res, 'TradeRejected', 'Trade offer rejected.', profile);
  }

  profile.money -= offeredAmount;
  profile.ownedBuildings.push(buildingId);
  applyConstructedSiteFromBuilding(profile, building);
  ensureBuildingStates(profile);
  savePlayerProfile(profile);
  return success(res, 'Trade offer accepted.', profile);
}
function handleSteal(req, res, payload) {
  const playerId = payload.playerId || 'player';
  const data = payload.data || {};
  console.log(`[server] action=steal playerId=${playerId} amount=${data.amount} canFail=${data.canFail}`);
  const amount = Number(data.amount) || 0;
  const canFail = !!data.canFail;
  const successChance = Number.isFinite(data.successChance) ? data.successChance : 70;

  const profile = loadPlayerProfile(playerId);

  if (!canFail) {
    profile.money += amount;
    savePlayerProfile(profile);
    return success(res, 'Steal success.', profile);
  }

  const roll = Math.floor(Math.random() * 100);
  const successRoll = roll < Math.max(0, Math.min(100, successChance));
  if (!successRoll) {
    return fail(res, 'StealFailed', 'Steal failed.', profile);
  }

  profile.money += amount;
  savePlayerProfile(profile);
  return success(res, 'Steal success.', profile);
}

function handleGetProfile(req, res, payload) {
  const playerId = payload.playerId || 'player';
  console.log(`[server] action=get_profile playerId=${playerId}`);
  const profile = loadPlayerProfile(playerId);
  ensureBuildingStates(profile);
  savePlayerProfile(profile);
  return success(res, 'Profile fetch success.', profile);
}

function handleSaveCheckpoint(req, res, payload) {
  const playerId = payload.playerId || 'player';
  const graphId = payload.data && payload.data.graphId;
  const checkpointId = payload.data && payload.data.checkpointId;
  console.log(`[server] action=save_checkpoint playerId=${playerId} graphId=${graphId} checkpointId=${checkpointId}`);

  if (!graphId) return fail(res, 'GraphIdEmpty', 'graphId is required.');

  const profile = loadPlayerProfile(playerId);
  if (!profile.graphCheckpoints || typeof profile.graphCheckpoints !== 'object') {
    profile.graphCheckpoints = {};
  }

  if (!checkpointId) {
    delete profile.graphCheckpoints[graphId];
    savePlayerProfile(profile);
    return success(res, 'Checkpoint cleared.', profile);
  }

  profile.graphCheckpoints[graphId] = checkpointId;
  savePlayerProfile(profile);
  return success(res, 'Checkpoint saved.', profile);
}

function handleAction(req, res, payload) {
  if (!payload || typeof payload !== 'object') {
    return fail(res, 'InvalidPayload', 'Invalid JSON payload.');
  }

  console.log('[server] action payload:', payload);
  switch (payload.action) {
    case 'buy_building':
      return handleBuyBuilding(req, res, payload);
    case 'start_quest':
      return handleStartQuest(req, res, payload);
    case 'complete_quest':
      return handleCompleteQuest(req, res, payload);
    case 'fail_quest':
      return handleFailQuest(req, res, payload);
    case 'add_money':
      return handleAddMoney(req, res, payload);
    case 'spend_money':
      return handleSpendMoney(req, res, payload);
    case 'steal':
      return handleSteal(req, res, payload);
    case 'get_profile':
      return handleGetProfile(req, res, payload);
    case 'save_checkpoint':
      return handleSaveCheckpoint(req, res, payload);
    case 'submit_trade_offer':
      return handleSubmitTradeOffer(req, res, payload);
    case 'rent_business': {
      console.log('[BusinessServer] action=rent_business');
      const profile = loadPlayerProfile(payload.playerId || 'player');
      const result = businessActions.rentBusiness(profile, payload.data, lotDefs);
      if (!result.ok) return fail(res, result.errorCode, result.message, profile);
      savePlayerProfile(profile);
      return success(res, result.message, profile);
    }
    case 'assign_business_type': {
      console.log('[BusinessServer] action=assign_business_type');
      const profile = loadPlayerProfile(payload.playerId || 'player');
      const result = businessActions.assignBusinessType(profile, payload.data, businessDefs, lotDefs);
      if (!result.ok) return fail(res, result.errorCode, result.message, profile);
      savePlayerProfile(profile);
      return success(res, result.message, profile);
    }
    case 'install_business_module': {
      console.log('[BusinessServer] action=install_business_module');
      const profile = loadPlayerProfile(payload.playerId || 'player');
      const result = businessActions.installBusinessModule(profile, payload.data, businessDefs);
      if (!result.ok) return fail(res, result.errorCode, result.message, profile);
      savePlayerProfile(profile);
      return success(res, result.message, profile);
    }
    case 'assign_supplier': {
      console.log('[BusinessServer] action=assign_supplier');
      const profile = loadPlayerProfile(payload.playerId || 'player');
      const result = businessActions.assignSupplier(profile, payload.data, businessDefs);
      if (!result.ok) return fail(res, result.errorCode, result.message, profile);
      savePlayerProfile(profile);
      return success(res, result.message, profile);
    }
    case 'hire_business_worker': {
      console.log('[BusinessServer] action=hire_business_worker');
      const profile = loadPlayerProfile(payload.playerId || 'player');
      const result = businessActions.hireBusinessWorker(profile, payload.data, businessDefs);
      if (!result.ok) return fail(res, result.errorCode, result.message, profile);
      savePlayerProfile(profile);
      return success(res, result.message, profile);
    }
    case 'open_business': {
      console.log('[BusinessServer] action=open_business');
      const profile = loadPlayerProfile(payload.playerId || 'player');
      const result = businessActions.openBusiness(profile, payload.data, businessDefs);
      if (!result.ok) return fail(res, result.errorCode, result.message, profile);
      savePlayerProfile(profile);
      return success(res, result.message, profile);
    }
    case 'close_business': {
      console.log('[BusinessServer] action=close_business');
      const profile = loadPlayerProfile(payload.playerId || 'player');
      const result = businessActions.closeBusiness(profile, payload.data);
      if (!result.ok) return fail(res, result.errorCode, result.message, profile);
      savePlayerProfile(profile);
      return success(res, result.message, profile);
    }
    case 'set_business_markup': {
      console.log('[BusinessServer] action=set_business_markup');
      const profile = loadPlayerProfile(payload.playerId || 'player');
      const result = businessActions.setBusinessMarkup(profile, payload.data);
      if (!result.ok) return fail(res, result.errorCode, result.message, profile);
      savePlayerProfile(profile);
      return success(res, result.message, profile);
    }
    case 'unlock_contact': {
      console.log('[BusinessServer] action=unlock_contact');
      const profile = loadPlayerProfile(payload.playerId || 'player');
      const result = businessActions.unlockContact(profile, payload.data);
      if (!result.ok) return fail(res, result.errorCode, result.message, profile);
      savePlayerProfile(profile);
      return success(res, result.message, profile);
    }
    case 'add_business_stock': {
      console.log('[BusinessServer] action=add_business_stock');
      const profile = loadPlayerProfile(payload.playerId || 'player');
      const result = businessActions.addBusinessStock(profile, payload.data);
      if (!result.ok) return fail(res, result.errorCode, result.message, profile);
      savePlayerProfile(profile);
      return success(res, result.message, profile);
    }
    case 'add_business_shelf_stock': {
      console.log('[BusinessServer] action=add_business_shelf_stock');
      const profile = loadPlayerProfile(payload.playerId || 'player');
      const result = businessActions.addBusinessShelfStock(profile, payload.data);
      if (!result.ok) return fail(res, result.errorCode, result.message, profile);
      savePlayerProfile(profile);
      return success(res, result.message, profile);
    }
    case 'clear_business_stock': {
      console.log('[BusinessServer] action=clear_business_stock');
      const profile = loadPlayerProfile(payload.playerId || 'player');
      const result = businessActions.clearBusinessStock(profile, payload.data);
      if (!result.ok) return fail(res, result.errorCode, result.message, profile);
      savePlayerProfile(profile);
      return success(res, result.message, profile);
    }
    case 'reset_businesses': {
      console.log('[BusinessServer] action=reset_businesses');
      const profile = loadPlayerProfile(payload.playerId || 'player');
      const result = businessActions.resetBusinesses(profile);
      if (!result.ok) return fail(res, result.errorCode, result.message, profile);
      savePlayerProfile(profile);
      return success(res, result.message, profile);
    }
    case 'construct_site_visual': {
      console.log('[BusinessServer] action=construct_site_visual');
      const profile = loadPlayerProfile(payload.playerId || 'player');
      const result = constructedSiteActions.constructSiteVisual(profile, payload.data);
      if (!result.ok) return fail(res, result.errorCode, result.message, profile);
      savePlayerProfile(profile);
      return success(res, result.message, profile);
    }
    case 'remove_site_visual': {
      console.log('[BusinessServer] action=remove_site_visual');
      const profile = loadPlayerProfile(payload.playerId || 'player');
      const result = constructedSiteActions.removeSiteVisual(profile, payload.data);
      if (!result.ok) return fail(res, result.errorCode, result.message, profile);
      savePlayerProfile(profile);
      return success(res, result.message, profile);
    }
    default:
      return fail(res, 'UnknownAction', `Unknown action: ${payload.action}`);
  }
}

const server = http.createServer((req, res) => {
  console.log('[server] incoming', req.method, req.url);
  if (req.method !== 'POST' || !req.url || !req.url.startsWith('/api/action')) {
    console.warn('[server] 404:', req.method, req.url);
    res.writeHead(404, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({
      success: false,
      errorCode: 'NotFound',
      message: 'Route not found'
    }));
    return;
  }

  let body = '';
  req.on('data', chunk => (body += chunk));
  req.on('end', () => {
    try {
      console.log('[server] raw body:', body);
      if (!body || !body.trim()) {
        console.warn('[server] empty body');
      }
      const payload = JSON.parse(body || '{}');
      try {
        handleAction(req, res, payload);
      } catch (err) {
        console.error('[server] Action handler error:', err && err.stack ? err.stack : err);
        respondJson(res, 200, {
          success: false,
          errorCode: 'ServerError',
          message: err && err.message ? err.message : 'Server error.'
        });
      }
    } catch (err) {
      respondJson(res, 200, {
        success: false,
        errorCode: 'InvalidJson',
        message: err.message
      });
    }
  });
});

server.listen(PORT, HOST, () => {
  console.log(`[server] Listening on http://${HOST}:${PORT}`);
});
