using UnityEngine;
using System.Collections.Generic;

public class BusinessQuestGraphRunner
{
    private readonly BusinessQuestGraph graph;
    private readonly GameBootstrap bootstrap;
    private readonly GameRuntimeState runtimeState;
    private readonly QuestService questService;
    private readonly PlayerService playerService;
    private readonly PlayerProfileState playerState;
    private readonly EventBus eventBus;
    private readonly DialogueService dialogueService;
    private readonly ChoiceUIService choiceUIService;
    private readonly MapMarkerService mapMarkerService;
    private readonly Transform playerTransform;
    private readonly GraphProgressService graphProgressService;

    private BusinessQuestNode currentNode;
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

        if (waitingConditionNode != null)
        {
            if (ConditionEvaluator.EvaluateCondition(waitingConditionNode, runtimeState, playerService, questService))
            {
                currentNode = graph.GetNodeById(waitingConditionNode.nextNodeId);
                waitingConditionNode = null;
                Advance();
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
                currentNode = graph.GetNodeById(waitingGoToNode.nextNodeId);
                waitingGoToNode = null;
                Advance();
            }
        }
    }

    private void Advance()
    {
        while (IsRunning && currentNode != null)
        {
            switch (currentNode)
            {
                case StartNode:
                    currentNode = graph.GetNodeById(currentNode.nextNodeId);
                    continue;

                case GiveQuestNode giveQuestNode:
                    if (giveQuestNode.questDefinition != null)
                    {
                        questService?.AcceptQuest(giveQuestNode.questDefinition);
                    }
                    currentNode = graph.GetNodeById(giveQuestNode.nextNodeId);
                    continue;

                case AddQuestNode addQuestNode:
                    if (addQuestNode.questDefinition != null)
                    {
                        questService?.AcceptQuest(addQuestNode.questDefinition);
                    }
                    currentNode = graph.GetNodeById(addQuestNode.nextNodeId);
                    continue;

                case CompleteQuestNode completeQuestNode:
                    if (!string.IsNullOrEmpty(completeQuestNode.questId))
                    {
                        questService?.CompleteQuest(completeQuestNode.questId);
                    }
                    currentNode = graph.GetNodeById(completeQuestNode.nextNodeId);
                    continue;

                case FailQuestNode failQuestNode:
                    if (!string.IsNullOrEmpty(failQuestNode.questId))
                    {
                        questService?.FailQuest(failQuestNode.questId);
                    }
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
                            currentNode = graph.GetNodeById(dialogueNode.nextNodeId);
                            Advance();
                        }, dialogueNode.screenshot);
                        return;
                    }
                    currentNode = graph.GetNodeById(dialogueNode.nextNodeId);
                    continue;

                case ChoiceNode choiceNode:
                    if (choiceUIService != null)
                    {
                        choiceUIService.ShowChoices(choiceNode.options, optionIndex =>
                        {
                            string nextId = GetChoiceNextId(choiceNode, optionIndex);
                            currentNode = graph.GetNodeById(nextId);
                            Advance();
                        });
                        return;
                    }
                    currentNode = graph.GetNodeById(GetChoiceNextId(choiceNode, 0));
                    continue;

                case SkillCheckNode skillCheckNode:
                    bool skillSuccess = CheckSkill(skillCheckNode);
                    currentNode = graph.GetNodeById(skillSuccess ? skillCheckNode.successNodeId : skillCheckNode.failNodeId);
                    continue;

                case ConditionNode conditionNode:
                    bool conditionResult = ConditionEvaluator.EvaluateCondition(conditionNode, runtimeState, playerService, questService);
                    currentNode = graph.GetNodeById(conditionResult ? conditionNode.trueNodeId : conditionNode.falseNodeId);
                    continue;

                case CheckpointNode checkpointNode:
                    SaveCheckpoint(checkpointNode);
                    currentNode = graph.GetNodeById(checkpointNode.nextNodeId);
                    continue;

                case BranchByInteractionContextNode branchNode:
                    currentNode = graph.GetNodeById(IsStealContext() ? branchNode.stealNodeId : branchNode.normalNodeId);
                    continue;

                case StealActionNode stealActionNode:
                    bool stealSuccess = ExecuteSteal(stealActionNode);
                    currentNode = graph.GetNodeById(stealSuccess ? stealActionNode.successNodeId : stealActionNode.failNodeId);
                    continue;

                case RaiseAlertNode raiseAlertNode:
                    if (!string.IsNullOrEmpty(raiseAlertNode.alertMessage))
                    {
                        Debug.Log(raiseAlertNode.alertMessage);
                    }
                    currentNode = graph.GetNodeById(raiseAlertNode.nextNodeId);
                    continue;

                case SpendMoneyNode spendMoneyNode:
                    if (playerService != null && playerService.HasEnoughMoney(spendMoneyNode.amount))
                    {
                        playerService.SpendMoney(spendMoneyNode.amount);
                        currentNode = graph.GetNodeById(spendMoneyNode.successNodeId);
                    }
                    else
                    {
                        currentNode = graph.GetNodeById(spendMoneyNode.failNodeId);
                    }
                    continue;

                case AddMapMarkerNode addMarkerNode:
                    mapMarkerService?.ShowMarker(addMarkerNode.markerId, addMarkerNode.targetTransform, addMarkerNode.title);
                    currentNode = graph.GetNodeById(addMarkerNode.nextNodeId);
                    continue;

                case SetGameObjectActiveNode setActiveNode:
                    ApplySetGameObjectActive(setActiveNode);
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
            Debug.Log("[SetGameObjectActive] Skipped: node or targetObject is null.");
            return;
        }

        string targetName = node.targetObject != null ? node.targetObject.name : "<null>";
        Debug.Log($"[SetGameObjectActive] Called: target='{targetName}', isActive={node.isActive}.");

        if (node.targetObject.scene.IsValid())
        {
            node.targetObject.SetActive(node.isActive);
            Debug.Log($"[SetGameObjectActive] Scene object '{targetName}' set active={node.isActive} (activeSelf={node.targetObject.activeSelf}).");
            return;
        }

        string key = !string.IsNullOrEmpty(node.spawnKey) ? node.spawnKey : node.id;
        if (string.IsNullOrEmpty(key))
        {
            Debug.Log($"[SetGameObjectActive] Skipped: empty spawnKey for prefab target='{targetName}'.");
            return;
        }

        if (node.isActive)
        {
            if (!spawnedObjects.TryGetValue(key, out GameObject instance) || instance == null)
            {
                instance = Object.Instantiate(node.targetObject);
                AssignBootstrap(instance);
                spawnedObjects[key] = instance;
                Debug.Log($"[SetGameObjectActive] Instantiated prefab '{targetName}' with key='{key}'.");
            }
            else
            {
                instance.SetActive(true);
                Debug.Log($"[SetGameObjectActive] Re-activated prefab instance '{targetName}' with key='{key}'.");
            }

            MarkBuildingOwnedIfNeeded(instance);
        }
        else
        {
            if (spawnedObjects.TryGetValue(key, out GameObject instance) && instance != null)
            {
                Object.Destroy(instance);
                Debug.Log($"[SetGameObjectActive] Destroyed prefab instance '{targetName}' with key='{key}'.");
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
            Debug.Log("[Checkpoint] No progress service, starting from StartNode.");
            return graph.GetStartNode();
        }

        string ownerId = GetOwnerId();
        string graphId = GetGraphId();
        if (string.IsNullOrEmpty(ownerId) || string.IsNullOrEmpty(graphId))
        {
            Debug.Log("[Checkpoint] Missing owner or graph id, starting from StartNode.");
            return graph.GetStartNode();
        }

        if (!graphProgressService.TryGetCheckpoint(ownerId, graphId, out string checkpointId) || string.IsNullOrEmpty(checkpointId))
        {
            Debug.Log($"[Checkpoint] No checkpoint for owner='{ownerId}', graph='{graphId}'. Start from StartNode.");
            return graph.GetStartNode();
        }

        CheckpointNode checkpoint = graph.GetCheckpointNodeById(checkpointId);
        if (checkpoint == null)
        {
            Debug.LogWarning($"[Checkpoint] Checkpoint '{checkpointId}' not found in graph '{graphId}'. Start from StartNode.");
            return graph.GetStartNode();
        }

        Debug.Log($"[Checkpoint] Resume from checkpoint '{checkpointId}' for owner='{ownerId}', graph='{graphId}'.");
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
            Debug.LogWarning("[Checkpoint] checkpointId is empty, not saved.");
            return;
        }

        string ownerId = GetOwnerId();
        string graphId = GetGraphId();
        if (string.IsNullOrEmpty(ownerId) || string.IsNullOrEmpty(graphId))
        {
            Debug.LogWarning("[Checkpoint] Missing owner or graph id, not saved.");
            return;
        }

        graphProgressService.SetCheckpoint(ownerId, graphId, node.checkpointId);
        Debug.Log($"[Checkpoint] Saved '{node.checkpointId}' for owner='{ownerId}', graph='{graphId}'.");
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
        Debug.Log($"[Checkpoint] Cleared for owner='{ownerId}', graph='{graphId}'.");
    }
}
