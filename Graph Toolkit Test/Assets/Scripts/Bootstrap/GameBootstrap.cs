using System.Collections.Generic;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [Header("Definitions")]
    public PlayerProfileDefinition playerDefinition;
    public List<BuildingDefinition> buildingDefinitions = new List<BuildingDefinition>();

    public GameRuntimeState RuntimeState { get; private set; }
    public EventBus EventBus { get; private set; }
    public PlayerService PlayerService { get; private set; }
    public BuildingService BuildingService { get; private set; }
    public QuestService QuestService { get; private set; }
    public GraphProgressService GraphProgressService { get; private set; }

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
