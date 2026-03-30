using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    public GameRuntimeState RuntimeState { get; private set; }
    public PlayerService PlayerService { get; private set; }
    public BuildingService BuildingService { get; private set; }
    public QuestService QuestService { get; private set; }
    public GraphProgressService GraphProgressService { get; private set; }
    public IGameServer GameServer { get; private set; }
    public ProfileSyncService ProfileSyncService { get; private set; }
    public RequestManager RequestManager { get; private set; }
    public GraphDebugService GraphDebugService { get; private set; }
    public GameDataRepository GameDataRepository { get; private set; }
    public PlayerStateSync PlayerStateSync { get; private set; }

    [Header("Game Data (JSON)")]
    public string gameDataFolder = "GameData";

    [Header("Server")]
    public bool useRemoteServer = false;
    public string remoteBaseUrl = "http://localhost:3000";
    public string remotePlayerId = "player";
    public float remoteTimeoutSeconds = 8f;
    public bool remoteDebugLog = true;

    [Header("Graph Debug")]
    public bool enableGraphDebug = true;
    public GraphDebugFilterMode graphDebugFilter = GraphDebugFilterMode.All;
    public int graphDebugMaxEvents = 200;
    public bool graphDebugLogToConsole = true;
    public bool graphDebugIncludeContext = true;

    private void Awake()
    {
        InitializeRuntime();
    }

    private void InitializeRuntime()
    {
        RuntimeState = new GameRuntimeState();

        QuestDatabaseData questDb = null;
        BuildingDatabaseData buildingDb = null;
        EconomyConfigData economy = null;

        string rootPath = Path.Combine(Application.streamingAssetsPath, gameDataFolder);
        Debug.Log($"[GameBootstrap] Loading JSON definitions from: {rootPath}");
        var loader = new JsonGameDataLoader(rootPath);
        questDb = loader.LoadQuests();
        buildingDb = loader.LoadBuildings();
        economy = loader.LoadEconomy();

        if (questDb == null)
        {
            Debug.LogError("[GameBootstrap] quests.json missing or invalid. Using empty quest database.");
            questDb = new QuestDatabaseData();
        }

        if (buildingDb == null)
        {
            Debug.LogError("[GameBootstrap] buildings.json missing or invalid. Using empty building database.");
            buildingDb = new BuildingDatabaseData();
        }

        if (economy == null)
        {
            Debug.LogError("[GameBootstrap] economy.json missing or invalid. Using empty economy config.");
            economy = new EconomyConfigData();
        }

        GameDataRepository = new GameDataRepository(questDb, buildingDb, economy);
        Debug.Log($"[GameBootstrap] GameDataRepository ready. Quests: {questDb.quests.Count}, Buildings: {buildingDb.buildings.Count}");

        RuntimeState.Player = new PlayerProfileState(GameDataRepository.GetEconomy());
        RuntimeState.Buildings = new List<BuildingState>();
        foreach (var definition in GameDataRepository.GetAllBuildings())
        {
            RuntimeState.Buildings.Add(new BuildingState(definition));
        }

        RuntimeState.Quests = new List<QuestState>();

        PlayerService = new PlayerService(RuntimeState.Player);
        BuildingService = new BuildingService();
        QuestService = new QuestService(RuntimeState);
        GraphProgressService = new GraphProgressService();
        GameServer = useRemoteServer
            ? new RemoteGameServer(remoteBaseUrl, remotePlayerId, remoteTimeoutSeconds, remoteDebugLog)
            : new LocalGameServer(RuntimeState, BuildingService, QuestService, GameDataRepository);
        PlayerStateSync = new PlayerStateSync();
        ProfileSyncService = new ProfileSyncService(RuntimeState, GameDataRepository, PlayerStateSync);
        RequestManager = new RequestManager();
        GraphDebugService = new GraphDebugService
        {
            Enabled = enableGraphDebug,
            FilterMode = graphDebugFilter,
            MaxEvents = graphDebugMaxEvents,
            LogToConsole = graphDebugLogToConsole,
            IncludeContextSnapshots = graphDebugIncludeContext
        };

        SeedInitialSnapshot();
    }

    private void SeedInitialSnapshot()
    {
        if (PlayerStateSync == null || RuntimeState == null)
        {
            return;
        }

        var snapshot = new ProfileSnapshot
        {
            Money = RuntimeState.Player != null ? RuntimeState.Player.Money : 0,
            Bargaining = RuntimeState.Player != null ? RuntimeState.Player.Bargaining : 0,
            Speech = RuntimeState.Player != null ? RuntimeState.Player.Speech : 0,
            Speed = RuntimeState.Player != null ? RuntimeState.Player.Speed : 0,
            Damage = RuntimeState.Player != null ? RuntimeState.Player.Damage : 0,
            Health = RuntimeState.Player != null ? RuntimeState.Player.Health : 0
        };

        if (RuntimeState.Quests != null)
        {
            foreach (var quest in RuntimeState.Quests)
            {
                if (quest == null || quest.Definition == null)
                {
                    continue;
                }

                if (quest.Status == QuestStatus.Active)
                {
                    snapshot.ActiveQuestIds.Add(quest.Definition.id);
                }
                else if (quest.Status == QuestStatus.Completed)
                {
                    snapshot.CompletedQuestIds.Add(quest.Definition.id);
                }
            }
        }

        if (RuntimeState.Buildings != null)
        {
            foreach (var building in RuntimeState.Buildings)
            {
                if (building == null || building.Definition == null)
                {
                    continue;
                }

                if (building.IsOwned)
                {
                    snapshot.OwnedBuildingIds.Add(building.Definition.id);
                }

                snapshot.BuildingStates.Add(new BuildingStateSnapshot
                {
                    id = building.Definition.id,
                    owned = building.IsOwned,
                    level = building.Level,
                    currentIncome = building.CurrentIncome,
                    currentExpenses = building.CurrentExpenses
                });
            }
        }

        PlayerStateSync.ApplySnapshot(snapshot);
    }

}
