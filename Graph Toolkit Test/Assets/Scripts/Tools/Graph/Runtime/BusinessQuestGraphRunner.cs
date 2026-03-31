using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class BusinessQuestGraphRunner
{
    private readonly BusinessQuestGraph graph;
    private readonly GameBootstrap bootstrap;
    private readonly IGameServer gameServer;
    private readonly ProfileSyncService profileSync;
    private readonly RequestManager requestManager;
    private readonly GraphDebugService debugService;
    private readonly GameDataRepository dataRepository;
    private readonly PlayerStateSync playerStateSync;
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
    private bool pendingEndClearCheckpoint;
    private int waitingUpgradeStartLevel = -1;
    private GoToPointNode waitingGoToNode;
    private readonly Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();
    private InteractionContext currentContext = new InteractionContext { contextType = InteractionContextType.Normal };

    public bool IsRunning { get; private set; }

    public BusinessQuestGraphRunner(
        BusinessQuestGraph graph,
        GameBootstrap bootstrap,
        IGameServer gameServer,
        DialogueService dialogueService,
        ChoiceUIService choiceUIService,
        MapMarkerService mapMarkerService,
        Transform playerTransform,
        GraphProgressService graphProgressService)
    {
        this.graph = graph;
        this.bootstrap = bootstrap;
        this.gameServer = gameServer;
        this.profileSync = bootstrap != null ? bootstrap.ProfileSyncService : null;
        this.requestManager = bootstrap != null ? bootstrap.RequestManager : null;
        this.debugService = bootstrap != null ? bootstrap.GraphDebugService : null;
        this.dataRepository = bootstrap != null ? bootstrap.GameDataRepository : null;
        this.playerStateSync = bootstrap != null ? bootstrap.PlayerStateSync : null;
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
        if (gameServer != null && currentNode != null)
        {
            bool canStart = requestManager == null || requestManager.TryStartRequest("RefreshProfile");
            if (canStart)
            {
                pendingServerTask = gameServer.TryGetProfileAsync();
                pendingSuccessNodeId = currentNode.id;
                pendingFailNodeId = currentNode.id;
                pendingRequestLabel = "RefreshProfile";
                Advance();
                return;
            }
        }

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

            bool isEndComplete = pendingRequestLabel == "EndCompleteQuest";

            if (result == null)
            {
                debugService?.LogNodeFail(GetGraphId(), currentNode, executionContext, "ServerResultNull", null, pendingFailNodeId);
                if (!isEndComplete)
                {
                    currentNode = graph.GetNodeById(pendingFailNodeId);
                }
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

                if (!isEndComplete)
                {
                    currentNode = graph.GetNodeById(result.Success ? pendingSuccessNodeId : pendingFailNodeId);
                }
            }

            requestManager?.FinishRequest();
            pendingServerTask = null;
            pendingSuccessNodeId = null;
            pendingFailNodeId = null;
            pendingRequestLabel = null;

            if (isEndComplete)
            {
                if (pendingEndClearCheckpoint)
                {
                    ClearCheckpoint();
                }
                pendingEndClearCheckpoint = false;
                Stop();
                return;
            }

            Advance();
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

                case ConditionNode conditionNode:
                    bool conditionResult = ConditionEvaluator.EvaluateCondition(conditionNode, playerStateSync);
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

                        pendingServerTask = gameServer.TryBuyBuildingAsync(
                            requestBuyBuildingNode.buildingId,
                            requestBuyBuildingNode.questAction,
                            requestBuyBuildingNode.questId);
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

                case EndNode endNode:
                    if (!string.IsNullOrWhiteSpace(endNode.completeQuestId))
                    {
                        Debug.Log($"[EndNode] START completeQuestId='{endNode.completeQuestId}'");
                        if (requestManager != null && !requestManager.TryStartRequest("EndCompleteQuest"))
                        {
                            debugService?.LogNodeFail(GetGraphId(), endNode, executionContext, "RequestBlocked", null, null);
                            Stop();
                            return;
                        }

                        if (gameServer == null)
                        {
                            Debug.Log("[EndNode] Result: Fail - ServerMissing");
                            requestManager?.FinishRequest();
                            debugService?.LogNodeFail(GetGraphId(), endNode, executionContext, "ServerMissing", null, null);
                            Stop();
                            return;
                        }

                        pendingServerTask = gameServer.TryCompleteQuestAsync(endNode.completeQuestId);
                        pendingSuccessNodeId = null;
                        pendingFailNodeId = null;
                        pendingEndClearCheckpoint = endNode.clearCheckpoint;
                        pendingRequestLabel = "EndCompleteQuest";
                        return;
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

            // Ownership is server-authoritative. Do not mutate local runtime state here.
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

    private bool IsStealContext()
    {
        return currentContext != null && currentContext.contextType == InteractionContextType.Steal;
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

            if (current is CheckpointNode)
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
            case CheckpointNode checkpointNode:
                SaveCheckpoint(checkpointNode);
                break;
        }
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
        waitingUpgradeStartLevel = -1;
        waitingGoToNode = null;
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
