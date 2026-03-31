const http = require('http');
const fs = require('fs');
const path = require('path');

const PORT = process.env.PORT ? Number(process.env.PORT) : 3000;
const DATA_DIR = path.join(__dirname, 'data');
const PLAYER_DIR = path.join(__dirname, 'playerData');

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

  const questById = new Map();
  (quests.quests || []).forEach(q => {
    if (q && q.id && !questById.has(q.id)) questById.set(q.id, q);
  });

  const buildingById = new Map();
  (buildings.buildings || []).forEach(b => {
    if (b && b.id && !buildingById.has(b.id)) buildingById.set(b.id, b);
  });

  return { questById, buildingById, economy };
}

const defs = (() => {
  try {
    return loadDefinitions();
  } catch (err) {
    console.error('[server] Failed to load definitions:', err.message);
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

  defs.buildingById.forEach((def, id) => {
    const state = existing.get(id);
    const owned = (profile.ownedBuildings || []).includes(id);
    const level = state && Number.isFinite(state.level) ? state.level : 0;
    const currentIncome = state && Number.isFinite(state.currentIncome) ? state.currentIncome : (def.incomePerDay || 0);
    const currentExpenses = state && Number.isFinite(state.currentExpenses) ? state.currentExpenses : (def.expensesPerDay || 0);

    profile.buildingStates.push({
      id,
      owned,
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
      speed: defs.economy && Number.isFinite(defs.economy.baseSpeed) ? defs.economy.baseSpeed : 0,
      damage: defs.economy && Number.isFinite(defs.economy.baseDamage) ? defs.economy.baseDamage : 0,
      health: defs.economy && Number.isFinite(defs.economy.baseHealth) ? defs.economy.baseHealth : 0,
      activeQuests: [],
      completedQuests: [],
      ownedBuildings: [],
      buildingStates: []
    };
    writeJson(filePath, profile);
    ensureBuildingStates(profile);
    return profile;
  }

  const profile = readJson(filePath);
  if (!profile.activeQuests) profile.activeQuests = [];
  if (!profile.completedQuests) profile.completedQuests = [];
  if (!profile.ownedBuildings) profile.ownedBuildings = [];
  if (!profile.buildingStates) profile.buildingStates = [];
  if (!Number.isFinite(profile.bargaining)) profile.bargaining = defs.economy?.baseBargaining ?? 0;
  if (!Number.isFinite(profile.speech)) profile.speech = defs.economy?.baseSpeech ?? 0;
  if (!Number.isFinite(profile.speed)) profile.speed = defs.economy?.baseSpeed ?? 0;
  if (!Number.isFinite(profile.damage)) profile.damage = defs.economy?.baseDamage ?? 0;
  if (!Number.isFinite(profile.health)) profile.health = defs.economy?.baseHealth ?? 0;
  ensureBuildingStates(profile);
  return profile;
}

function savePlayerProfile(profile) {
  const id = safePlayerId(profile.playerId);
  const filePath = path.join(PLAYER_DIR, `${id}.json`);
  ensureBuildingStates(profile);
  writeJson(filePath, profile);
}

function toResponseProfile(profile) {
  return {
    money: profile.money,
    bargaining: profile.bargaining,
    speech: profile.speech,
    speed: profile.speed,
    damage: profile.damage,
    health: profile.health,
    activeQuests: profile.activeQuests || [],
    completedQuests: profile.completedQuests || [],
    buildings: profile.ownedBuildings || [],
    buildingStates: profile.buildingStates || []
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
  console.log(`[server] action=buy_building playerId=${playerId} buildingId=${buildingId}`);
  if (!buildingId) return fail(res, 'BuildingIdEmpty', 'buildingId is required.');

  const building = defs.buildingById.get(buildingId);
  if (!building) return fail(res, 'BuildingNotFound', 'Building not found.');

  const profile = loadPlayerProfile(playerId);

  if ((profile.ownedBuildings || []).includes(buildingId)) {
    return fail(res, 'BuildingAlreadyOwned', 'Building already owned.', profile);
  }

  const cost = Number(building.purchaseCost) || 0;
  if (profile.money < cost) {
    return fail(res, 'NotEnoughMoney', 'Not enough money.', profile);
  }

  profile.money -= cost;
  profile.ownedBuildings.push(buildingId);
  ensureBuildingStates(profile);
  savePlayerProfile(profile);
  return success(res, 'Buy building success.', profile);
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

function handleAction(req, res, payload) {
  if (!payload || typeof payload !== 'object') {
    return fail(res, 'InvalidPayload', 'Invalid JSON payload.');
  }

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
    default:
      return fail(res, 'UnknownAction', `Unknown action: ${payload.action}`);
  }
}

const server = http.createServer((req, res) => {
  if (req.method !== 'POST' || req.url !== '/api/action') {
    res.writeHead(404);
    res.end('Not found');
    return;
  }

  let body = '';
  req.on('data', chunk => (body += chunk));
  req.on('end', () => {
    try {
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

server.listen(PORT, () => {
  console.log(`[server] Listening on http://localhost:${PORT}`);
});
