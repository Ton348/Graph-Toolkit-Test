using System.Collections.Generic;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [Header("Definitions")]
    public PlayerProfileDefinition playerDefinition;
    public List<BuildingDefinition> buildingDefinitions = new List<BuildingDefinition>();
    public List<QuestDefinition> questDefinitions = new List<QuestDefinition>();

    public GameRuntimeState RuntimeState { get; private set; }
    public EventBus EventBus { get; private set; }
    public PlayerService PlayerService { get; private set; }
    public BuildingService BuildingService { get; private set; }
    public QuestService QuestService { get; private set; }
    public GraphProgressService GraphProgressService { get; private set; }
    public IGameServer GameServer { get; private set; }
    public ProfileSyncService ProfileSyncService { get; private set; }
    public RequestManager RequestManager { get; private set; }
    public GraphDebugService GraphDebugService { get; private set; }

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
        RuntimeState.Player = new PlayerProfileState(playerDefinition);

        RuntimeState.Buildings = new List<BuildingState>();
        foreach (BuildingDefinition definition in buildingDefinitions)
        {
            RuntimeState.Buildings.Add(new BuildingState(definition));
        }

        RuntimeState.Quests = new List<QuestState>();

        EventBus = new EventBus();
        PlayerService = new PlayerService(RuntimeState.Player);
        BuildingService = new BuildingService(EventBus);
        QuestService = new QuestService(RuntimeState, EventBus);
        GraphProgressService = new GraphProgressService();
        GameServer = useRemoteServer
            ? new RemoteGameServer(remoteBaseUrl, remotePlayerId, remoteTimeoutSeconds, remoteDebugLog)
            : new LocalGameServer(RuntimeState, BuildingService, QuestService, questDefinitions);
        ProfileSyncService = new ProfileSyncService(RuntimeState, questDefinitions, buildingDefinitions);
        RequestManager = new RequestManager();
        GraphDebugService = new GraphDebugService
        {
            Enabled = enableGraphDebug,
            FilterMode = graphDebugFilter,
            MaxEvents = graphDebugMaxEvents,
            LogToConsole = graphDebugLogToConsole,
            IncludeContextSnapshots = graphDebugIncludeContext
        };
    }

    public BuildingState GetBuildingState(BuildingDefinition definition)
    {
        if (RuntimeState == null || definition == null || RuntimeState.Buildings == null)
        {
            return null;
        }

        foreach (BuildingState state in RuntimeState.Buildings)
        {
            if (state.Definition == definition)
            {
                return state;
            }
        }

        return null;
    }
}
