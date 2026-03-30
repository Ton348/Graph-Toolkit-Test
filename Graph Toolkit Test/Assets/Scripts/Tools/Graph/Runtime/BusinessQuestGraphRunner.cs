using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class BusinessQuestGraphRunner
{
    private readonly BusinessQuestGraph graph;
    private readonly GameBootstrap bootstrap;
    private readonly GameRuntimeState runtimeState;
    private readonly QuestService questService;
    private readonly PlayerService playerService;
    private readonly PlayerProfileState playerState;
    private readonly IGameServer gameServer;
    private readonly ProfileSyncService profileSync;
    private readonly RequestManager requestManager;
    private readonly GraphDebugService debugService;
    private readonly EventBus eventBus;
    private readonly DialogueService dialogueService;
    private readonly ChoiceUIService choiceUIService;
    private readonly MapMarkerService mapMarkerService;
    private readonly Transform playerTransform;
    private readonly GraphProgressService graphProgressService;

    private BusinessQuestNode currentNode;
    private GraphExecutionContext executionContext;
    private Task<ServerActionResult> pendingServerTask;
    private string pendingSuccessNodeId;
    private string pendingFailNodeId;
    private string pendingRequestLabel;
    private WaitForBuildingPurchasedNode waitingPurchaseNode;
    private WaitForBuildingUpgradedNode waitingUpgradeNode;
    private GoToPointNode waitingGoToNode;
    private WaitForConditionNode waitingConditionNode;
    private readonly Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();
    private InteractionContext currentContext = new InteractionContext { contextType = InteractionContextType.Normal };

    public bool IsRunning { get; private set; }

    public BusinessQuestGraphRunner(
        BusinessQuestGraph graph,
        GameBootstrap bootstrap,
        GameRuntimeState runtimeState,
        QuestService questService,
        PlayerService playerService,
        PlayerProfileState playerState,
        IGameServer gameServer,
        EventBus eventBus,
        DialogueService dialogueService,
        ChoiceUIService choiceUIService,
        MapMarkerService mapMarkerService,
        Transform playerTransform,
        GraphProgressService graphProgressService)
    {
        this.graph = graph;
        this.bootstrap = bootstrap;
        this.runtimeState = runtimeState;
        this.questService = questService;
        this.playerService = playerService;
        this.playerState = playerState;
        this.gameServer = gameServer;
        this.profileSync = bootstrap != null ? bootstrap.ProfileSyncService : null;
        this.requestManager = bootstrap != null ? bootstrap.RequestManager : null;
        this.debugService = bootstrap != null ? bootstrap.GraphDebugService : null;
        this.eventBus = eventBus;
        this.dialogueService = dialogueService;
        this.choiceUIService = choiceUIService;
        this.mapMarkerService = mapMarkerService;
        this.playerTransform = playerTransform;
        this.graphProgressService = graphProgressService;
    }

    public void Start()
    {
        Start(new InteractionContext { contextType = InteractionContextType.Normal });
    }

    public void Start(InteractionContext context)
    {
        if (IsRunning || graph == null)
        {
            return;
        }

        currentContext = context ?? new InteractionContext { contextType = InteractionContextType.Normal };
        executionContext = new GraphExecutionContext();
        debugService?.Clear();
        IsRunning = true;
        currentNode = ResolveStartNode();
        Advance();
    }

    public void Tick()
    {
        if (!IsRunning)
        {
            return;
        }

        if (pendingServerTask != null)
        {
            if (!pendingServerTask.IsCompleted)
            {
                return;
            }

            ServerActionResult result = null;
            try
            {
                result = pendingServerTask.Result;
            }
            catch
            {
                Debug.Log($"[{pendingRequestLabel}] Result: Fail - Exception");
            }

            if (result == null)
            {
                debugService?.LogNodeFail(GetGraphId(), currentNode, executionContext, "ServerResultNull", null, pendingFailNodeId);
                currentNode = graph.GetNodeById(pendingFailNodeId);
            }
            else
            {
                executionContext?.Set(GraphContextKeys.ServerLastResult, result);

                if (result.ProfileSnapshot != null && profileSync != null)
                {
                    profileSync.ApplySnapshot(result.ProfileSnapshot);
                    Debug.Log($"[{pendingRequestLabel}] Profile snapshot applied");
                }

                if (result.Success)
                {
                    Debug.Log($"[{pendingRequestLabel}] Result: Success");
                    debugService?.LogNodeSuccess(GetGraphId(), currentNode, executionContext, "Success", result, pendingSuccessNodeId);
                }
                else
                {
                    Debug.Log($"[{pendingRequestLabel}] Result: {result.Type} - {result.ErrorCode}");
                    debugService?.LogNodeFail(GetGraphId(), currentNode, executionContext, $"{result.Type} - {result.ErrorCode}", result, pendingFailNodeId);
                }

                currentNode = graph.GetNodeById(result.Success ? pendingSuccessNodeId : pendingFailNodeId);
            }

            requestManager?.FinishRequest();
            pendingServerTask = null;
            pendingSuccessNodeId = null;
            pendingFailNodeId = null;
            pendingRequestLabel = null;

            Advance();
            return;
        }

        if (waitingConditionNode != null)
        {
            if (ConditionEvaluator.EvaluateCondition(waitingConditionNode, runtimeState, playerService, questService))
            {
                executionContext?.Set(GraphContextKeys.ConditionLastResult, true);
                debugService?.LogNodeSuccess(GetGraphId(), waitingConditionNode, executionContext, "True", null, waitingConditionNode.nextNodeId);
                currentNode = graph.GetNodeById(waitingConditionNode.nextNodeId);
                waitingConditionNode = null;
                Advance();
            }
            else
            {
                executionContext?.Set(GraphContextKeys.ConditionLastResult, false);
            }

            return;
        }

        if (waitingGoToNode != null)
        {
            Transform target = GetTargetTransform(waitingGoToNode);
            if (target == null || playerTransform == null)
            {
                return;
            }

            float distance = Vector3.Distance(playerTransform.position, target.position);
            if (distance <= waitingGoToNode.arrivalDistance)
            {
                debugService?.LogNodeSuccess(GetGraphId(), waitingGoToNode, executionContext, "Arrived", null, waitingGoToNode.nextNodeId);
                currentNode = graph.GetNodeById(waitingGoToNode.nextNodeId);
                waitingGoToNode = null;
                Advance();
            }
        }
    }

    private void Advance()
    {
        if (pendingServerTask != null)
        {
            return;
        }

        while (IsRunning && currentNode != null)
        {
            debugService?.LogNodeStart(GetGraphId(), currentNode, executionContext);
            switch (currentNode)
            {
                case StartNode:
                    debugService?.LogNodeSuccess(GetGraphId(), currentNode, executionContext, null, null, currentNode.nextNodeId);
                    currentNode = graph.GetNodeById(currentNode.nextNodeId);
                    continue;

                case GiveQuestNode giveQuestNode:
                    if (giveQuestNode.questDefinition != null)
                    {
                        questService?.AcceptQuest(giveQuestNode.questDefinition);
                    }
                    debugService?.LogNodeSuccess(GetGraphId(), giveQuestNode, executionContext, null, null, giveQuestNode.nextNodeId);
                    currentNode = graph.GetNodeById(giveQuestNode.nextNodeId);
                    continue;

                case AddQuestNode addQuestNode:
                    if (addQuestNode.questDefinition != null)
                    {
                        questService?.AcceptQuest(addQuestNode.questDefinition);
                    }
                    debugService?.LogNodeSuccess(GetGraphId(), addQuestNode, executionContext, null, null, addQuestNode.nextNodeId);
                    currentNode = graph.GetNodeById(addQuestNode.nextNodeId);
                    continue;

                case CompleteQuestNode completeQuestNode:
                    if (!string.IsNullOrEmpty(completeQuestNode.questId))
                    {
                        questService?.CompleteQuest(completeQuestNode.questId);
                    }
                    debugService?.LogNodeSuccess(GetGraphId(), completeQuestNode, executionContext, null, null, completeQuestNode.nextNodeId);
                    currentNode = graph.GetNodeById(completeQuestNode.nextNodeId);
                    continue;

                case FailQuestNode failQuestNode:
                    if (!string.IsNullOrEmpty(failQuestNode.questId))
                    {
                        questService?.FailQuest(failQuestNode.questId);
                    }
                    debugService?.LogNodeSuccess(GetGraphId(), failQuestNode, executionContext, null, null, failQuestNode.nextNodeId);
                    currentNode = graph.GetNodeById(failQuestNode.nextNodeId);
                    continue;

                case DialogueNode dialogueNode:
                    if (dialogueService != null)
                    {
                        if (TryShowDialogueWithImmediateChoice(dialogueNode))
                        {
                            return;
                        }

                        dialogueService.ShowDialogue(dialogueNode.title, dialogueNode.bodyText, () =>
                        {
                            debugService?.LogNodeSuccess(GetGraphId(), dialogueNode, executionContext, "Closed", null, dialogueNode.nextNodeId);
                            currentNode = graph.GetNodeById(dialogueNode.nextNodeId);
                            Advance();
                        }, dialogueNode.screenshot);
                        return;
                    }
                    debugService?.LogNodeSuccess(GetGraphId(), dialogueNode, executionContext, "Skipped", null, dialogueNode.nextNodeId);
                    currentNode = graph.GetNodeById(dialogueNode.nextNodeId);
                    continue;

                case ChoiceNode choiceNode:
                    if (choiceUIService != null)
                    {
                        choiceUIService.ShowChoices(choiceNode.options, optionIndex =>
                        {
                            StoreChoiceContext(choiceNode, optionIndex);
                            debugService?.LogNodeSuccess(GetGraphId(), choiceNode, executionContext, $"Choice {optionIndex}", null, GetChoiceNextId(choiceNode, optionIndex));
                            string nextId = GetChoiceNextId(choiceNode, optionIndex);
                            currentNode = graph.GetNodeById(nextId);
                            Advance();
                        });
                        return;
                    }
                    StoreChoiceContext(choiceNode, 0);
                    debugService?.LogNodeSuccess(GetGraphId(), choiceNode, executionContext, "DefaultChoice 0", null, GetChoiceNextId(choiceNode, 0));
                    currentNode = graph.GetNodeById(GetChoiceNextId(choiceNode, 0));
                    continue;

                case SkillCheckNode skillCheckNode:
                    bool skillSuccess = CheckSkill(skillCheckNode);
                    if (skillSuccess)
                    {
                        debugService?.LogNodeSuccess(GetGraphId(), skillCheckNode, executionContext, "Success", null, skillCheckNode.successNodeId);
                    }
                    else
                    {
                        debugService?.LogNodeFail(GetGraphId(), skillCheckNode, executionContext, "Fail", null, skillCheckNode.failNodeId);
                    }
                    currentNode = graph.GetNodeById(skillSuccess ? skillCheckNode.successNodeId : skillCheckNode.failNodeId);
                    continue;

                case ConditionNode conditionNode:
                    bool conditionResult = ConditionEvaluator.EvaluateCondition(conditionNode, runtimeState, playerService, questService);
                    executionContext?.Set(GraphContextKeys.ConditionLastResult, conditionResult);
                    if (conditionResult)
                    {
                        debugService?.LogNodeSuccess(GetGraphId(), conditionNode, executionContext, "True", null, conditionNode.trueNodeId);
                    }
                    else
                    {
                        debugService?.LogNodeFail(GetGraphId(), conditionNode, executionContext, "False", null, conditionNode.falseNodeId);
                    }
                    currentNode = graph.GetNodeById(conditionResult ? conditionNode.trueNodeId : conditionNode.falseNodeId);
                    continue;

                case CheckpointNode checkpointNode:
                    SaveCheckpoint(checkpointNode);
                    debugService?.LogNodeSuccess(GetGraphId(), checkpointNode, executionContext, null, null, checkpointNode.nextNodeId);
                    currentNode = graph.GetNodeById(checkpointNode.nextNodeId);
                    continue;

                case BranchByInteractionContextNode branchNode:
                    debugService?.LogNodeSuccess(GetGraphId(), branchNode, executionContext, IsStealContext() ? "Steal" : "Normal", null, IsStealContext() ? branchNode.stealNodeId : branchNode.normalNodeId);
                    currentNode = graph.GetNodeById(IsStealContext() ? branchNode.stealNodeId : branchNode.normalNodeId);
                    continue;

                case StealActionNode stealActionNode:
                    bool stealSuccess = ExecuteSteal(stealActionNode);
                    if (stealSuccess)
                    {
                        debugService?.LogNodeSuccess(GetGraphId(), stealActionNode, executionContext, "Success", null, stealActionNode.successNodeId);
                    }
                    else
                    {
                        debugService?.LogNodeFail(GetGraphId(), stealActionNode, executionContext, "Fail", null, stealActionNode.failNodeId);
                    }
                    currentNode = graph.GetNodeById(stealSuccess ? stealActionNode.successNodeId : stealActionNode.failNodeId);
                    continue;

                case RaiseAlertNode raiseAlertNode:
                    debugService?.LogNodeSuccess(GetGraphId(), raiseAlertNode, executionContext, null, null, raiseAlertNode.nextNodeId);
                    currentNode = graph.GetNodeById(raiseAlertNode.nextNodeId);
                    continue;

                case SpendMoneyNode spendMoneyNode:
                    if (playerService == null)
                    {
                        debugService?.LogNodeFail(GetGraphId(), spendMoneyNode, executionContext, "PlayerServiceMissing", null, spendMoneyNode.failNodeId);
                        currentNode = graph.GetNodeById(spendMoneyNode.failNodeId);
                        continue;
                    }

                    if (spendMoneyNode.operation == MoneyOperation.Give)
                    {
                        playerService.AddMoney(spendMoneyNode.amount);
                        debugService?.LogNodeSuccess(GetGraphId(), spendMoneyNode, executionContext, "Give", null, spendMoneyNode.successNodeId);
                        currentNode = graph.GetNodeById(spendMoneyNode.successNodeId);
                        continue;
                    }

                    if (playerService.HasEnoughMoney(spendMoneyNode.amount))
                    {
                        playerService.SpendMoney(spendMoneyNode.amount);
                        debugService?.LogNodeSuccess(GetGraphId(), spendMoneyNode, executionContext, "Spend", null, spendMoneyNode.successNodeId);
                        currentNode = graph.GetNodeById(spendMoneyNode.successNodeId);
                    }
                    else
                    {
                        debugService?.LogNodeFail(GetGraphId(), spendMoneyNode, executionContext, "NotEnoughMoney", null, spendMoneyNode.failNodeId);
                        currentNode = graph.GetNodeById(spendMoneyNode.failNodeId);
                    }
                    continue;

                case RequestBuyBuildingNode requestBuyBuildingNode:
                    {
                        Debug.Log($"[RequestBuyBuilding] START buildingId='{requestBuyBuildingNode.buildingId}'");
                        executionContext?.Set(GraphContextKeys.BuildingLastRequestedId, requestBuyBuildingNode.buildingId);
                        if (requestManager != null && !requestManager.TryStartRequest("RequestBuyBuilding"))
                        {
                            debugService?.LogNodeFail(GetGraphId(), requestBuyBuildingNode, executionContext, "RequestBlocked", null, requestBuyBuildingNode.failNodeId);
                            currentNode = graph.GetNodeById(requestBuyBuildingNode.failNodeId);
                            continue;
                        }

                        if (gameServer == null)
                        {
                            Debug.Log("[RequestBuyBuilding] Result: Fail - ServerMissing");
                            debugService?.LogNodeFail(GetGraphId(), requestBuyBuildingNode, executionContext, "ServerMissing", null, requestBuyBuildingNode.failNodeId);
                            requestManager?.FinishRequest();
                            currentNode = graph.GetNodeById(requestBuyBuildingNode.failNodeId);
                            continue;
                        }

                        pendingServerTask = gameServer.TryBuyBuildingAsync(requestBuyBuildingNode.buildingId);
                        pendingSuccessNodeId = requestBuyBuildingNode.successNodeId;
                        pendingFailNodeId = requestBuyBuildingNode.failNodeId;
                        pendingRequestLabel = "RequestBuyBuilding";
                        return;
                    }

                case RequestStartQuestNode requestStartQuestNode:
                    {
                        Debug.Log($"[RequestStartQuest] START questId='{requestStartQuestNode.questId}'");
                        executionContext?.Set(GraphContextKeys.QuestLastRequestedId, requestStartQuestNode.questId);
                        if (requestManager != null && !requestManager.TryStartRequest("RequestStartQuest"))
                        {
                            debugService?.LogNodeFail(GetGraphId(), requestStartQuestNode, executionContext, "RequestBlocked", null, requestStartQuestNode.failNodeId);
                            currentNode = graph.GetNodeById(requestStartQuestNode.failNodeId);
                            continue;
                        }

                        if (gameServer == null)
                        {
                            Debug.Log("[RequestStartQuest] Result: Fail - ServerMissing");
                            debugService?.LogNodeFail(GetGraphId(), requestStartQuestNode, executionContext, "ServerMissing", null, requestStartQuestNode.failNodeId);
                            requestManager?.FinishRequest();
                            currentNode = graph.GetNodeById(requestStartQuestNode.failNodeId);
                            continue;
                        }

                        pendingServerTask = gameServer.TryStartQuestAsync(requestStartQuestNode.questId);
                        pendingSuccessNodeId = requestStartQuestNode.successNodeId;
                        pendingFailNodeId = requestStartQuestNode.failNodeId;
                        pendingRequestLabel = "RequestStartQuest";
                        return;
                    }

                case RequestCompleteQuestNode requestCompleteQuestNode:
                    {
                        Debug.Log($"[RequestCompleteQuest] START questId='{requestCompleteQuestNode.questId}'");
                        executionContext?.Set(GraphContextKeys.QuestLastRequestedId, requestCompleteQuestNode.questId);
                        if (requestManager != null && !requestManager.TryStartRequest("RequestCompleteQuest"))
                        {
                            debugService?.LogNodeFail(GetGraphId(), requestCompleteQuestNode, executionContext, "RequestBlocked", null, requestCompleteQuestNode.failNodeId);
                            currentNode = graph.GetNodeById(requestCompleteQuestNode.failNodeId);
                            continue;
                        }

                        if (gameServer == null)
                        {
                            Debug.Log("[RequestCompleteQuest] Result: Fail - ServerMissing");
                            debugService?.LogNodeFail(GetGraphId(), requestCompleteQuestNode, executionContext, "ServerMissing", null, requestCompleteQuestNode.failNodeId);
                            requestManager?.FinishRequest();
                            currentNode = graph.GetNodeById(requestCompleteQuestNode.failNodeId);
                            continue;
                        }

                        pendingServerTask = gameServer.TryCompleteQuestAsync(requestCompleteQuestNode.questId);
                        pendingSuccessNodeId = requestCompleteQuestNode.successNodeId;
                        pendingFailNodeId = requestCompleteQuestNode.failNodeId;
                        pendingRequestLabel = "RequestCompleteQuest";
                        return;
                    }

                case AddMapMarkerNode addMarkerNode:
                    mapMarkerService?.ShowMarker(addMarkerNode.markerId, addMarkerNode.targetTransform, addMarkerNode.title);
                    if (CompassManager.Instance != null)
                    {
                        CompassManager.Instance.ShowTarget(addMarkerNode.markerId);
                    }
                    Debug.Log($"[Marker] Node issued markerId='{addMarkerNode.markerId}' title='{addMarkerNode.title}'");
                    debugService?.LogNodeSuccess(GetGraphId(), addMarkerNode, executionContext, null, null, addMarkerNode.nextNodeId);
                    currentNode = graph.GetNodeById(addMarkerNode.nextNodeId);
                    continue;

                case SetGameObjectActiveNode setActiveNode:
                    ApplySetGameObjectActive(setActiveNode);
                    debugService?.LogNodeSuccess(GetGraphId(), setActiveNode, executionContext, null, null, setActiveNode.nextNodeId);
                    currentNode = graph.GetNodeById(setActiveNode.nextNodeId);
                    continue;

                case GoToPointNode goToPointNode:
                    waitingGoToNode = goToPointNode;
                    return;

                case WaitForBuildingPurchasedNode waitPurchase:
                    waitingPurchaseNode = waitPurchase;
                    eventBus?.Subscribe<BuildingPurchasedEvent>(OnBuildingPurchased);
                    return;

                case WaitForBuildingUpgradedNode waitUpgrade:
                    waitingUpgradeNode = waitUpgrade;
                    eventBus?.Subscribe<BuildingUpgradedEvent>(OnBuildingUpgraded);
                    return;

                case WaitForConditionNode waitCondition:
                    waitingConditionNode = waitCondition;
                    return;

                case EndNode endNode:
                    if (!string.IsNullOrWhiteSpace(endNode.completeQuestId))
                    {
                        questService?.CompleteQuest(endNode.completeQuestId);
                    }
                    if (endNode.clearCheckpoint)
                    {
                        ClearCheckpoint();
                    }
                    debugService?.LogNodeSuccess(GetGraphId(), endNode, executionContext, "End", null, null);
                    Stop();
                    return;
            }

            Stop();
            return;
        }

        Stop();
    }

    private void OnBuildingPurchased(BuildingPurchasedEvent evt)
    {
        if (waitingPurchaseNode == null)
        {
            return;
        }

        if (!IsBuildingMatch(evt.Building, waitingPurchaseNode.buildingId))
        {
            return;
        }

        eventBus?.Unsubscribe<BuildingPurchasedEvent>(OnBuildingPurchased);
        debugService?.LogNodeSuccess(GetGraphId(), waitingPurchaseNode, executionContext, "Purchased", null, waitingPurchaseNode.nextNodeId);
        currentNode = graph.GetNodeById(waitingPurchaseNode.nextNodeId);
        waitingPurchaseNode = null;
        Advance();
    }

    private void OnBuildingUpgraded(BuildingUpgradedEvent evt)
    {
        if (waitingUpgradeNode == null)
        {
            return;
        }

        if (!IsBuildingMatch(evt.Building, waitingUpgradeNode.buildingId))
        {
            return;
        }

        eventBus?.Unsubscribe<BuildingUpgradedEvent>(OnBuildingUpgraded);
        debugService?.LogNodeSuccess(GetGraphId(), waitingUpgradeNode, executionContext, "Upgraded", null, waitingUpgradeNode.nextNodeId);
        currentNode = graph.GetNodeById(waitingUpgradeNode.nextNodeId);
        waitingUpgradeNode = null;
        Advance();
    }

    private bool IsBuildingMatch(BuildingState building, string buildingId)
    {
        if (building == null || building.Definition == null)
        {
            return false;
        }

        if (string.IsNullOrEmpty(buildingId))
        {
            return true;
        }

        return building.Definition.buildingId == buildingId;
    }

    private Transform GetTargetTransform(GoToPointNode node)
    {
        if (node == null)
        {
            return null;
        }

        if (node.targetTransform != null)
        {
            return node.targetTransform;
        }

        return mapMarkerService != null ? mapMarkerService.GetTarget(node.markerId) : null;
    }

    private string GetChoiceNextId(ChoiceNode node, int optionIndex)
    {
        if (node == null || node.options == null || node.options.Count == 0)
        {
            return null;
        }

        if (optionIndex < 0 || optionIndex >= node.options.Count)
        {
            optionIndex = 0;
        }

        ChoiceOption selected = node.options[optionIndex];
        if (selected != null)
        {
            return selected.nextNodeId;
        }

        foreach (ChoiceOption option in node.options)
        {
            if (option != null)
            {
                return option.nextNodeId;
            }
        }

        return null;
    }

    private void ApplySetGameObjectActive(SetGameObjectActiveNode node)
    {
        if (node == null || node.targetObject == null)
        {
            return;
        }

        if (node.targetObject.scene.IsValid())
        {
            node.targetObject.SetActive(node.isActive);
            return;
        }

        string key = !string.IsNullOrEmpty(node.spawnKey) ? node.spawnKey : node.id;
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        if (node.isActive)
        {
            if (!spawnedObjects.TryGetValue(key, out GameObject instance) || instance == null)
            {
                instance = Object.Instantiate(node.targetObject);
                AssignBootstrap(instance);
                spawnedObjects[key] = instance;
            }
            else
            {
                instance.SetActive(true);
            }

            MarkBuildingOwnedIfNeeded(instance);
        }
        else
        {
            if (spawnedObjects.TryGetValue(key, out GameObject instance) && instance != null)
            {
                Object.Destroy(instance);
            }

            spawnedObjects.Remove(key);
        }
    }

    private void AssignBootstrap(GameObject instance)
    {
        if (instance == null || bootstrap == null)
        {
            return;
        }

        var buildingInteractables = instance.GetComponentsInChildren<BuildingInteractable>(true);
        if (buildingInteractables != null)
        {
            foreach (var interactable in buildingInteractables)
            {
                if (interactable != null)
                {
                    interactable.bootstrap = bootstrap;
                }
            }
        }

        var npcManagers = instance.GetComponentsInChildren<NPCManager>(true);
        if (npcManagers != null)
        {
            foreach (var npc in npcManagers)
            {
                if (npc != null)
                {
                    npc.bootstrap = bootstrap;
                }
            }
        }
    }

    private void MarkBuildingOwnedIfNeeded(GameObject instance)
    {
        if (instance == null || bootstrap == null || runtimeState == null)
        {
            return;
        }

        var buildingInteractables = instance.GetComponentsInChildren<BuildingInteractable>(true);
        if (buildingInteractables == null || buildingInteractables.Length == 0)
        {
            return;
        }

        foreach (var interactable in buildingInteractables)
        {
            if (interactable == null || interactable.definition == null)
            {
                continue;
            }

            BuildingState state = bootstrap.GetBuildingState(interactable.definition);
            if (state == null)
            {
                continue;
            }

            if (!state.IsOwned)
            {
                state.IsOwned = true;
                eventBus?.Publish(new BuildingPurchasedEvent(state));
            }
        }
    }

    private bool IsStealContext()
    {
        return currentContext != null && currentContext.contextType == InteractionContextType.Steal;
    }

    private bool ExecuteSteal(StealActionNode node)
    {
        if (node == null)
        {
            return false;
        }

        if (!node.canFail)
        {
            playerService?.AddMoney(node.stealAmount);
            return true;
        }

        int chance = 70;
        int roll = Random.Range(0, 100);
        bool success = roll < chance;
        if (success)
        {
            playerService?.AddMoney(node.stealAmount);
        }

        return success;
    }

    private BusinessQuestNode ResolveStartNode()
    {
        if (graphProgressService == null)
        {
            return graph.GetStartNode();
        }

        string ownerId = GetOwnerId();
        string graphId = GetGraphId();
        if (string.IsNullOrEmpty(ownerId) || string.IsNullOrEmpty(graphId))
        {
            return graph.GetStartNode();
        }

        if (!graphProgressService.TryGetCheckpoint(ownerId, graphId, out string checkpointId) || string.IsNullOrEmpty(checkpointId))
        {
            return graph.GetStartNode();
        }

        CheckpointNode checkpoint = graph.GetCheckpointNodeById(checkpointId);
        if (checkpoint == null)
        {
            return graph.GetStartNode();
        }

        return graph.GetNodeById(checkpoint.nextNodeId) ?? checkpoint;
    }

    private void SaveCheckpoint(CheckpointNode node)
    {
        if (node == null || graphProgressService == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(node.checkpointId))
        {
            return;
        }

        string ownerId = GetOwnerId();
        string graphId = GetGraphId();
        if (string.IsNullOrEmpty(ownerId) || string.IsNullOrEmpty(graphId))
        {
            return;
        }

        graphProgressService.SetCheckpoint(ownerId, graphId, node.checkpointId);
    }

    private string GetOwnerId()
    {
        if (currentContext != null && currentContext.sourceNpc != null)
        {
            if (!string.IsNullOrEmpty(currentContext.sourceNpc.ownerId))
            {
                return currentContext.sourceNpc.ownerId;
            }

            return currentContext.sourceNpc.name;
        }

        return "Global";
    }

    private string GetGraphId()
    {
        return graph != null ? graph.name : string.Empty;
    }

    private bool TryShowDialogueWithImmediateChoice(DialogueNode dialogueNode)
    {
        if (dialogueNode == null || dialogueService == null || choiceUIService == null)
        {
            return false;
        }

        if (!TryFindImmediateChoice(dialogueNode, out ChoiceNode choiceNode))
        {
            return false;
        }

        if (choiceNode.options == null || choiceNode.options.Count == 0)
        {
            return false;
        }

        dialogueService.ShowDialogue(dialogueNode.title, dialogueNode.bodyText, null, dialogueNode.screenshot);
        choiceUIService.ShowChoices(choiceNode.options, optionIndex =>
        {
            string nextId = GetChoiceNextId(choiceNode, optionIndex);
            debugService?.LogNodeSuccess(GetGraphId(), choiceNode, executionContext, $"Choice {optionIndex}", null, nextId);
            currentNode = graph.GetNodeById(nextId);
            Advance();
        });
        return true;
    }

    private bool TryFindImmediateChoice(DialogueNode dialogueNode, out ChoiceNode choiceNode)
    {
        choiceNode = null;
        if (dialogueNode == null || string.IsNullOrEmpty(dialogueNode.nextNodeId))
        {
            return false;
        }

        BusinessQuestNode current = graph.GetNodeById(dialogueNode.nextNodeId);
        int safety = 0;
        while (current != null && safety < 20)
        {
            if (current is ChoiceNode foundChoice)
            {
                choiceNode = foundChoice;
                return true;
            }

            if (current is GiveQuestNode or AddQuestNode or CheckpointNode)
            {
                ExecuteImmediateNode(current);
                current = graph.GetNodeById(current.nextNodeId);
                safety++;
                continue;
            }

            return false;
        }

        return false;
    }

    private void StoreChoiceContext(ChoiceNode choiceNode, int optionIndex)
    {
        if (executionContext == null || choiceNode == null)
        {
            return;
        }

        executionContext.Set(GraphContextKeys.ChoiceLastIndex, optionIndex);

        string label = string.Empty;
        if (choiceNode.options != null && optionIndex >= 0 && optionIndex < choiceNode.options.Count)
        {
            label = choiceNode.options[optionIndex]?.label ?? string.Empty;
        }

        executionContext.Set(GraphContextKeys.ChoiceLastLabel, label);
    }

    private void ExecuteImmediateNode(BusinessQuestNode node)
    {
        switch (node)
        {
            case GiveQuestNode giveQuestNode:
                if (giveQuestNode.questDefinition != null)
                {
                    questService?.AcceptQuest(giveQuestNode.questDefinition);
                }
                break;
            case AddQuestNode addQuestNode:
                if (addQuestNode.questDefinition != null)
                {
                    questService?.AcceptQuest(addQuestNode.questDefinition);
                }
                break;
            case CheckpointNode checkpointNode:
                SaveCheckpoint(checkpointNode);
                break;
        }
    }

    private bool CheckSkill(SkillCheckNode node)
    {
        if (node == null || playerState == null)
        {
            return false;
        }

        int value = 0;
        switch (node.skillType)
        {
            case SkillType.Bargaining:
                value = playerState.Bargaining;
                break;
            case SkillType.Speech:
                value = playerState.Speech;
                break;
            case SkillType.Speed:
                value = playerState.Speed;
                break;
            case SkillType.Damage:
                value = playerState.Damage;
                break;
            case SkillType.Health:
                value = playerState.Health;
                break;
        }

        return value >= node.requiredValue;
    }

    public void Stop()
    {
        if (!IsRunning)
        {
            return;
        }

        requestManager?.FinishRequest();
        pendingServerTask = null;
        pendingSuccessNodeId = null;
        pendingFailNodeId = null;
        pendingRequestLabel = null;
        executionContext?.Clear();
        executionContext = null;
        dialogueService?.HideDialogue();
        choiceUIService?.HideChoices();
        eventBus?.Unsubscribe<BuildingPurchasedEvent>(OnBuildingPurchased);
        eventBus?.Unsubscribe<BuildingUpgradedEvent>(OnBuildingUpgraded);
        waitingPurchaseNode = null;
        waitingUpgradeNode = null;
        waitingGoToNode = null;
        waitingConditionNode = null;
        currentNode = null;
        IsRunning = false;
    }

    private void ClearCheckpoint()
    {
        if (graphProgressService == null)
        {
            return;
        }

        string ownerId = GetOwnerId();
        string graphId = GetGraphId();
        if (string.IsNullOrEmpty(ownerId) || string.IsNullOrEmpty(graphId))
        {
            return;
        }

        graphProgressService.ClearCheckpoint(ownerId, graphId);
    }
}
