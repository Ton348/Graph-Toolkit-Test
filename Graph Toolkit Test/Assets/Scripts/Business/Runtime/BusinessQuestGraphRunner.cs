using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseFlow = GraphCore.BaseNodes.Runtime.Flow;
using BaseUI = GraphCore.BaseNodes.Runtime.UI;
using BaseUtility = GraphCore.BaseNodes.Runtime.Utility;

public class BusinessQuestGraphRunner
{
    private readonly BusinessQuestGraph graph;
    private readonly GameBootstrap bootstrap;
    private readonly IGameServer gameServer;
    private readonly ProfileSyncService profileSync;
    private readonly RequestManager requestManager;
    private readonly GameDataRepository dataRepository;
    private readonly PlayerStateSync playerStateSync;
    private readonly BusinessStateSyncService businessStateSync;
    private readonly DialogueService dialogueService;
    private readonly ChoiceUIService choiceUIService;
    private readonly TradeOfferUIService tradeOfferUIService;
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
    private bool delayPending;
    private float delayUntilTime;
    private string delayNextNodeId;
    private int waitingUpgradeStartLevel = -1;
    private GoToPointNode waitingGoToNode;
    private InteractionContext currentContext = new InteractionContext { contextType = InteractionContextType.Normal };

    public bool IsRunning { get; private set; }
    public bool HasTradeOfferUI => tradeOfferUIService != null;

    public BusinessQuestGraphRunner(
        BusinessQuestGraph graph,
        GameBootstrap bootstrap,
        IGameServer gameServer,
        DialogueService dialogueService,
        ChoiceUIService choiceUIService,
        TradeOfferUIService tradeOfferUIService,
        MapMarkerService mapMarkerService,
        Transform playerTransform,
        GraphProgressService graphProgressService)
    {
        this.graph = graph;
        this.bootstrap = bootstrap;
        this.gameServer = gameServer;
        this.profileSync = bootstrap != null ? bootstrap.ProfileSyncService : null;
        this.requestManager = bootstrap != null ? bootstrap.RequestManager : null;
        this.dataRepository = bootstrap != null ? bootstrap.GameDataRepository : null;
        this.playerStateSync = bootstrap != null ? bootstrap.PlayerStateSync : null;
        this.businessStateSync = bootstrap != null ? bootstrap.BusinessStateSyncService : null;
        this.dialogueService = dialogueService;
        this.choiceUIService = choiceUIService;
        this.tradeOfferUIService = tradeOfferUIService;
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

            bool isEndComplete = pendingRequestLabel == "EndCompleteQuest";

            if (result == null)
            {
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
                }
                else
                {
                    Debug.Log($"[{pendingRequestLabel}] Result: {result.Type} - {result.ErrorCode}");
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

        if (delayPending)
        {
            if (Time.time < delayUntilTime)
            {
                return;
            }

            delayPending = false;
            currentNode = graph.GetNodeById(delayNextNodeId);
            delayNextNodeId = null;
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
            switch (currentNode)
            {
                case BaseFlow.StartNode:
                    currentNode = graph.GetNodeById(currentNode.nextNodeId);
                    continue;

                case BaseFlow.FinishNode:
                    Stop();
                    return;

                case BaseUI.DialogueNode baseDialogueNode:
                    if (dialogueService != null)
                    {
                        dialogueService.ShowDialogue(baseDialogueNode.dialogueTitle, baseDialogueNode.body, () =>
                        {
                            currentNode = graph.GetNodeById(baseDialogueNode.nextNodeId);
                            Advance();
                        });
                        return;
                    }
                    currentNode = graph.GetNodeById(baseDialogueNode.nextNodeId);
                    continue;

                case BaseUI.ChoiceNode baseChoiceNode:
                    if (choiceUIService != null)
                    {
                        choiceUIService.ShowChoices(ToLegacyChoices(baseChoiceNode.options), optionIndex =>
                        {
                            currentNode = graph.GetNodeById(GetBaseChoiceNextId(baseChoiceNode, optionIndex));
                            Advance();
                        });
                        return;
                    }
                    currentNode = graph.GetNodeById(GetBaseChoiceNextId(baseChoiceNode, 0));
                    continue;

                case BaseUtility.LogNode logNode:
                    Debug.Log(logNode.message ?? string.Empty);
                    currentNode = graph.GetNodeById(logNode.nextNodeId);
                    continue;

                case BaseFlow.DelayNode delayNode:
                    if (delayNode.delaySeconds <= 0f)
                    {
                        currentNode = graph.GetNodeById(delayNode.nextNodeId);
                        continue;
                    }

                    delayPending = true;
                    delayUntilTime = Time.time + delayNode.delaySeconds;
                    delayNextNodeId = delayNode.nextNodeId;
                    return;

                case BaseFlow.RandomNode randomNode:
                    currentNode = graph.GetNodeById(GetRandomNextId(randomNode));
                    continue;

                case StartNode:
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
                            StoreChoiceContext(choiceNode, optionIndex);
                            string nextId = GetChoiceNextId(choiceNode, optionIndex);
                            currentNode = graph.GetNodeById(nextId);
                            Advance();
                        });
                        return;
                    }
                    StoreChoiceContext(choiceNode, 0);
                    currentNode = graph.GetNodeById(GetChoiceNextId(choiceNode, 0));
                    continue;

                case ConditionNode conditionNode:
                    bool conditionResult = ConditionEvaluator.EvaluateCondition(conditionNode, playerStateSync);
                    executionContext?.Set(GraphContextKeys.ConditionLastResult, conditionResult);
                    if (conditionResult)
                    {
                    }
                    else
                    {
                    }
                    currentNode = graph.GetNodeById(conditionResult ? conditionNode.trueNodeId : conditionNode.falseNodeId);
                    continue;

                case CheckBusinessExistsNode checkBusinessExistsNode:
                    {
                        bool result = businessStateSync != null && businessStateSync.HasBusiness(checkBusinessExistsNode.lotId);
                        Debug.Log($"[BusinessGraph] CheckBusinessExists lotId='{checkBusinessExistsNode.lotId}' -> {result}");
                        currentNode = graph.GetNodeById(result ? checkBusinessExistsNode.trueNodeId : checkBusinessExistsNode.falseNodeId);
                        continue;
                    }

                case CheckBusinessOpenNode checkBusinessOpenNode:
                    {
                        bool result = businessStateSync != null && businessStateSync.IsBusinessOpen(checkBusinessOpenNode.lotId);
                        Debug.Log($"[BusinessGraph] CheckBusinessOpen lotId='{checkBusinessOpenNode.lotId}' -> {result}");
                        currentNode = graph.GetNodeById(result ? checkBusinessOpenNode.trueNodeId : checkBusinessOpenNode.falseNodeId);
                        continue;
                    }

                case CheckBusinessModuleInstalledNode checkBusinessModuleInstalledNode:
                    {
                        bool result = businessStateSync != null && businessStateSync.HasModule(checkBusinessModuleInstalledNode.lotId, checkBusinessModuleInstalledNode.moduleId);
                        Debug.Log($"[BusinessGraph] CheckBusinessModuleInstalled lotId='{checkBusinessModuleInstalledNode.lotId}' moduleId='{checkBusinessModuleInstalledNode.moduleId}' -> {result}");
                        currentNode = graph.GetNodeById(result ? checkBusinessModuleInstalledNode.trueNodeId : checkBusinessModuleInstalledNode.falseNodeId);
                        continue;
                    }

                case CheckContactKnownNode checkContactKnownNode:
                    {
                        bool result = businessStateSync != null && businessStateSync.HasKnownContact(checkContactKnownNode.contactId);
                        Debug.Log($"[BusinessGraph] CheckContactKnown contactId='{checkContactKnownNode.contactId}' -> {result}");
                        currentNode = graph.GetNodeById(result ? checkContactKnownNode.trueNodeId : checkContactKnownNode.falseNodeId);
                        continue;
                    }

                case CheckpointNode checkpointNode:
                    {
                        Debug.Log($"[Checkpoint] START checkpointId='{checkpointNode.checkpointId}'");
                        SaveCheckpoint(checkpointNode);
                        if (pendingServerTask != null)
                        {
                            return;
                        }
                        currentNode = graph.GetNodeById(checkpointNode.nextNodeId);
                        continue;
                    }

                case RequestBuyBuildingNode requestBuyBuildingNode:
                    {
                        Debug.Log($"[RequestBuyBuilding] START buildingId='{requestBuyBuildingNode.buildingId}'");
                        executionContext?.Set(GraphContextKeys.BuildingLastRequestedId, requestBuyBuildingNode.buildingId);
                        if (requestManager != null && !requestManager.TryStartRequest("RequestBuyBuilding"))
                        {
                            currentNode = graph.GetNodeById(requestBuyBuildingNode.failNodeId);
                            continue;
                        }

                        if (gameServer == null)
                        {
                            Debug.Log("[RequestBuyBuilding] Result: Fail - ServerMissing");
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

                case RequestTradeOfferNode requestTradeOfferNode:
                    {
                        if (tradeOfferUIService == null)
                        {
                            Debug.Log("[RequestTradeOffer] Result: Fail - UI missing");
                            currentNode = graph.GetNodeById(requestTradeOfferNode.failNodeId);
                            continue;
                        }

                        if (tradeOfferUIService.IsOpen)
                        {
                            return;
                        }

                        string buildingId = requestTradeOfferNode.buildingId;
                        if (string.IsNullOrEmpty(buildingId))
                        {
                            Debug.Log("[RequestTradeOffer] Result: Fail - BuildingIdEmpty");
                            currentNode = graph.GetNodeById(requestTradeOfferNode.failNodeId);
                            continue;
                        }

                        var buildingDef = dataRepository != null ? dataRepository.GetBuildingById(buildingId) : null;
                        if (buildingDef == null)
                        {
                            Debug.Log("[RequestTradeOffer] Result: Fail - BuildingNotFound");
                            currentNode = graph.GetNodeById(requestTradeOfferNode.failNodeId);
                            continue;
                        }

                        string label = !string.IsNullOrEmpty(buildingDef.displayName) ? buildingDef.displayName : buildingDef.id;
                        int fullPrice = Mathf.Max(1, buildingDef.purchaseCost);
                        tradeOfferUIService.ShowOffer(label, fullPrice, offeredAmount =>
                        {
                            if (requestManager != null && !requestManager.TryStartRequest("RequestTradeOffer"))
                            {
                                Debug.Log("[RequestTradeOffer] Result: Fail - RequestBlocked");
                                currentNode = graph.GetNodeById(requestTradeOfferNode.failNodeId);
                                Advance();
                                return;
                            }

                            if (gameServer == null)
                            {
                                Debug.Log("[RequestTradeOffer] Result: Fail - ServerMissing");
                                requestManager?.FinishRequest();
                                currentNode = graph.GetNodeById(requestTradeOfferNode.failNodeId);
                                Advance();
                                return;
                            }

                            Debug.Log($"[RequestTradeOffer] START buildingId='{buildingId}' offeredAmount='{offeredAmount}'");
                            executionContext?.Set(GraphContextKeys.BuildingLastRequestedId, buildingId);
                            pendingServerTask = gameServer.TrySubmitTradeOfferAsync(buildingId, offeredAmount);
                            pendingSuccessNodeId = requestTradeOfferNode.successNodeId;
                            pendingFailNodeId = requestTradeOfferNode.failNodeId;
                            pendingRequestLabel = "RequestTradeOffer";
                        });
                        return;
                    }

                case RequestStartQuestNode requestStartQuestNode:
                    {
                        Debug.Log($"[RequestStartQuest] START questId='{requestStartQuestNode.questId}'");
                        executionContext?.Set(GraphContextKeys.QuestLastRequestedId, requestStartQuestNode.questId);
                        if (requestManager != null && !requestManager.TryStartRequest("RequestStartQuest"))
                        {
                            currentNode = graph.GetNodeById(requestStartQuestNode.failNodeId);
                            continue;
                        }

                        if (gameServer == null)
                        {
                            Debug.Log("[RequestStartQuest] Result: Fail - ServerMissing");
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
                            currentNode = graph.GetNodeById(requestCompleteQuestNode.failNodeId);
                            continue;
                        }

                        if (gameServer == null)
                        {
                            Debug.Log("[RequestCompleteQuest] Result: Fail - ServerMissing");
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

                case RequestRentBusinessNode requestRentBusinessNode:
                    {
                        Debug.Log($"[BusinessGraph] RequestRentBusiness START lotId='{requestRentBusinessNode.lotId}'");
                        if (requestManager != null && !requestManager.TryStartRequest("RequestRentBusiness"))
                        {
                            currentNode = graph.GetNodeById(requestRentBusinessNode.failNodeId);
                            continue;
                        }

                        if (gameServer == null)
                        {
                            Debug.Log("[BusinessGraph] RequestRentBusiness Result: Fail - ServerMissing");
                            requestManager?.FinishRequest();
                            currentNode = graph.GetNodeById(requestRentBusinessNode.failNodeId);
                            continue;
                        }

                        pendingServerTask = gameServer.TryRentBusinessAsync(requestRentBusinessNode.lotId);
                        pendingSuccessNodeId = requestRentBusinessNode.successNodeId;
                        pendingFailNodeId = requestRentBusinessNode.failNodeId;
                        pendingRequestLabel = "RequestRentBusiness";
                        return;
                    }

                case RequestAssignBusinessTypeNode requestAssignBusinessTypeNode:
                    {
                        Debug.Log($"[BusinessGraph] RequestAssignBusinessType START lotId='{requestAssignBusinessTypeNode.lotId}' businessTypeId='{requestAssignBusinessTypeNode.businessTypeId}'");
                        if (requestManager != null && !requestManager.TryStartRequest("RequestAssignBusinessType"))
                        {
                            currentNode = graph.GetNodeById(requestAssignBusinessTypeNode.failNodeId);
                            continue;
                        }

                        if (gameServer == null)
                        {
                            Debug.Log("[BusinessGraph] RequestAssignBusinessType Result: Fail - ServerMissing");
                            requestManager?.FinishRequest();
                            currentNode = graph.GetNodeById(requestAssignBusinessTypeNode.failNodeId);
                            continue;
                        }

                        pendingServerTask = gameServer.TryAssignBusinessTypeAsync(requestAssignBusinessTypeNode.lotId, requestAssignBusinessTypeNode.businessTypeId);
                        pendingSuccessNodeId = requestAssignBusinessTypeNode.successNodeId;
                        pendingFailNodeId = requestAssignBusinessTypeNode.failNodeId;
                        pendingRequestLabel = "RequestAssignBusinessType";
                        return;
                    }

                case RequestInstallBusinessModuleNode requestInstallBusinessModuleNode:
                    {
                        Debug.Log($"[BusinessGraph] RequestInstallBusinessModule START lotId='{requestInstallBusinessModuleNode.lotId}' moduleId='{requestInstallBusinessModuleNode.moduleId}'");
                        if (requestManager != null && !requestManager.TryStartRequest("RequestInstallBusinessModule"))
                        {
                            currentNode = graph.GetNodeById(requestInstallBusinessModuleNode.failNodeId);
                            continue;
                        }

                        if (gameServer == null)
                        {
                            Debug.Log("[BusinessGraph] RequestInstallBusinessModule Result: Fail - ServerMissing");
                            requestManager?.FinishRequest();
                            currentNode = graph.GetNodeById(requestInstallBusinessModuleNode.failNodeId);
                            continue;
                        }

                        pendingServerTask = gameServer.TryInstallBusinessModuleAsync(requestInstallBusinessModuleNode.lotId, requestInstallBusinessModuleNode.moduleId);
                        pendingSuccessNodeId = requestInstallBusinessModuleNode.successNodeId;
                        pendingFailNodeId = requestInstallBusinessModuleNode.failNodeId;
                        pendingRequestLabel = "RequestInstallBusinessModule";
                        return;
                    }

                case RequestAssignSupplierNode requestAssignSupplierNode:
                    {
                        Debug.Log($"[BusinessGraph] RequestAssignSupplier START lotId='{requestAssignSupplierNode.lotId}' supplierId='{requestAssignSupplierNode.supplierId}'");
                        if (requestManager != null && !requestManager.TryStartRequest("RequestAssignSupplier"))
                        {
                            currentNode = graph.GetNodeById(requestAssignSupplierNode.failNodeId);
                            continue;
                        }

                        if (gameServer == null)
                        {
                            Debug.Log("[BusinessGraph] RequestAssignSupplier Result: Fail - ServerMissing");
                            requestManager?.FinishRequest();
                            currentNode = graph.GetNodeById(requestAssignSupplierNode.failNodeId);
                            continue;
                        }

                        pendingServerTask = gameServer.TryAssignSupplierAsync(requestAssignSupplierNode.lotId, requestAssignSupplierNode.supplierId);
                        pendingSuccessNodeId = requestAssignSupplierNode.successNodeId;
                        pendingFailNodeId = requestAssignSupplierNode.failNodeId;
                        pendingRequestLabel = "RequestAssignSupplier";
                        return;
                    }

                case RequestHireBusinessWorkerNode requestHireBusinessWorkerNode:
                    {
                        Debug.Log($"[BusinessGraph] RequestHireBusinessWorker START lotId='{requestHireBusinessWorkerNode.lotId}' roleId='{requestHireBusinessWorkerNode.roleId}' contactId='{requestHireBusinessWorkerNode.contactId}'");
                        if (requestManager != null && !requestManager.TryStartRequest("RequestHireBusinessWorker"))
                        {
                            currentNode = graph.GetNodeById(requestHireBusinessWorkerNode.failNodeId);
                            continue;
                        }

                        if (gameServer == null)
                        {
                            Debug.Log("[BusinessGraph] RequestHireBusinessWorker Result: Fail - ServerMissing");
                            requestManager?.FinishRequest();
                            currentNode = graph.GetNodeById(requestHireBusinessWorkerNode.failNodeId);
                            continue;
                        }

                        pendingServerTask = gameServer.TryHireBusinessWorkerAsync(requestHireBusinessWorkerNode.lotId, requestHireBusinessWorkerNode.roleId, requestHireBusinessWorkerNode.contactId);
                        pendingSuccessNodeId = requestHireBusinessWorkerNode.successNodeId;
                        pendingFailNodeId = requestHireBusinessWorkerNode.failNodeId;
                        pendingRequestLabel = "RequestHireBusinessWorker";
                        return;
                    }

                case RequestSetBusinessOpenNode requestSetBusinessOpenNode:
                    {
                        var actionLabel = requestSetBusinessOpenNode.open ? "Open" : "Close";
                        Debug.Log($"[BusinessGraph] RequestSetBusinessOpen START action='{actionLabel}' lotId='{requestSetBusinessOpenNode.lotId}'");
                        if (requestManager != null && !requestManager.TryStartRequest("RequestSetBusinessOpen"))
                        {
                            currentNode = graph.GetNodeById(requestSetBusinessOpenNode.failNodeId);
                            continue;
                        }

                        if (gameServer == null)
                        {
                            Debug.Log("[BusinessGraph] RequestSetBusinessOpen Result: Fail - ServerMissing");
                            requestManager?.FinishRequest();
                            currentNode = graph.GetNodeById(requestSetBusinessOpenNode.failNodeId);
                            continue;
                        }

                        pendingServerTask = requestSetBusinessOpenNode.open
                            ? gameServer.TryOpenBusinessAsync(requestSetBusinessOpenNode.lotId)
                            : gameServer.TryCloseBusinessAsync(requestSetBusinessOpenNode.lotId);
                        pendingSuccessNodeId = requestSetBusinessOpenNode.successNodeId;
                        pendingFailNodeId = requestSetBusinessOpenNode.failNodeId;
                        pendingRequestLabel = "RequestSetBusinessOpen";
                        return;
                    }

                case RequestOpenBusinessNode requestOpenBusinessNode:
                    {
                        Debug.Log($"[BusinessGraph] RequestOpenBusiness START lotId='{requestOpenBusinessNode.lotId}'");
                        if (requestManager != null && !requestManager.TryStartRequest("RequestOpenBusiness"))
                        {
                            currentNode = graph.GetNodeById(requestOpenBusinessNode.failNodeId);
                            continue;
                        }

                        if (gameServer == null)
                        {
                            Debug.Log("[BusinessGraph] RequestOpenBusiness Result: Fail - ServerMissing");
                            requestManager?.FinishRequest();
                            currentNode = graph.GetNodeById(requestOpenBusinessNode.failNodeId);
                            continue;
                        }

                        pendingServerTask = gameServer.TryOpenBusinessAsync(requestOpenBusinessNode.lotId);
                        pendingSuccessNodeId = requestOpenBusinessNode.successNodeId;
                        pendingFailNodeId = requestOpenBusinessNode.failNodeId;
                        pendingRequestLabel = "RequestOpenBusiness";
                        return;
                    }

                case RequestCloseBusinessNode requestCloseBusinessNode:
                    {
                        Debug.Log($"[BusinessGraph] RequestCloseBusiness START lotId='{requestCloseBusinessNode.lotId}'");
                        if (requestManager != null && !requestManager.TryStartRequest("RequestCloseBusiness"))
                        {
                            currentNode = graph.GetNodeById(requestCloseBusinessNode.failNodeId);
                            continue;
                        }

                        if (gameServer == null)
                        {
                            Debug.Log("[BusinessGraph] RequestCloseBusiness Result: Fail - ServerMissing");
                            requestManager?.FinishRequest();
                            currentNode = graph.GetNodeById(requestCloseBusinessNode.failNodeId);
                            continue;
                        }

                        pendingServerTask = gameServer.TryCloseBusinessAsync(requestCloseBusinessNode.lotId);
                        pendingSuccessNodeId = requestCloseBusinessNode.successNodeId;
                        pendingFailNodeId = requestCloseBusinessNode.failNodeId;
                        pendingRequestLabel = "RequestCloseBusiness";
                        return;
                    }

                case RequestSetBusinessMarkupNode requestSetBusinessMarkupNode:
                    {
                        Debug.Log($"[BusinessGraph] RequestSetBusinessMarkup START lotId='{requestSetBusinessMarkupNode.lotId}' markupPercent='{requestSetBusinessMarkupNode.markupPercent}'");
                        if (requestManager != null && !requestManager.TryStartRequest("RequestSetBusinessMarkup"))
                        {
                            currentNode = graph.GetNodeById(requestSetBusinessMarkupNode.failNodeId);
                            continue;
                        }

                        if (gameServer == null)
                        {
                            Debug.Log("[BusinessGraph] RequestSetBusinessMarkup Result: Fail - ServerMissing");
                            requestManager?.FinishRequest();
                            currentNode = graph.GetNodeById(requestSetBusinessMarkupNode.failNodeId);
                            continue;
                        }

                        pendingServerTask = gameServer.TrySetBusinessMarkupAsync(requestSetBusinessMarkupNode.lotId, requestSetBusinessMarkupNode.markupPercent);
                        pendingSuccessNodeId = requestSetBusinessMarkupNode.successNodeId;
                        pendingFailNodeId = requestSetBusinessMarkupNode.failNodeId;
                        pendingRequestLabel = "RequestSetBusinessMarkup";
                        return;
                    }

                case RequestUnlockContactNode requestUnlockContactNode:
                    {
                        Debug.Log($"[BusinessGraph] RequestUnlockContact START contactId='{requestUnlockContactNode.contactId}'");
                        if (requestManager != null && !requestManager.TryStartRequest("RequestUnlockContact"))
                        {
                            currentNode = graph.GetNodeById(requestUnlockContactNode.failNodeId);
                            continue;
                        }

                        if (gameServer == null)
                        {
                            Debug.Log("[BusinessGraph] RequestUnlockContact Result: Fail - ServerMissing");
                            requestManager?.FinishRequest();
                            currentNode = graph.GetNodeById(requestUnlockContactNode.failNodeId);
                            continue;
                        }

                        pendingServerTask = gameServer.TryUnlockContactAsync(requestUnlockContactNode.contactId);
                        pendingSuccessNodeId = requestUnlockContactNode.successNodeId;
                        pendingFailNodeId = requestUnlockContactNode.failNodeId;
                        pendingRequestLabel = "RequestUnlockContact";
                        return;
                    }

                case RefreshProfileNode refreshProfileNode:
                    {
                        Debug.Log("[RefreshProfile] START");
                        if (requestManager != null && !requestManager.TryStartRequest("RefreshProfile"))
                        {
                            currentNode = graph.GetNodeById(refreshProfileNode.failNodeId);
                            continue;
                        }

                        if (gameServer == null)
                        {
                            Debug.Log("[RefreshProfile] Result: Fail - ServerMissing");
                            requestManager?.FinishRequest();
                            currentNode = graph.GetNodeById(refreshProfileNode.failNodeId);
                            continue;
                        }

                        pendingServerTask = gameServer.TryGetProfileAsync();
                        pendingSuccessNodeId = refreshProfileNode.successNodeId;
                        pendingFailNodeId = refreshProfileNode.failNodeId;
                        pendingRequestLabel = "RefreshProfile";
                        return;
                    }

                case AddMapMarkerNode addMarkerNode:
                    mapMarkerService?.ShowMarker(addMarkerNode.markerId, addMarkerNode.targetTransform, addMarkerNode.title);
                    if (CompassManager.Instance != null)
                    {
                        CompassManager.Instance.ShowTarget(addMarkerNode.markerId);
                    }
                    Debug.Log($"[Marker] Node issued markerId='{addMarkerNode.markerId}' title='{addMarkerNode.title}'");
                    currentNode = graph.GetNodeById(addMarkerNode.nextNodeId);
                    continue;

                case SetGameObjectActiveNode setActiveNode:
                    if (TryRequestSetGameObjectActive(setActiveNode))
                    {
                        return;
                    }
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
                            Stop();
                            return;
                        }

                        if (gameServer == null)
                        {
                            Debug.Log("[EndNode] Result: Fail - ServerMissing");
                            requestManager?.FinishRequest();
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

    private static List<ChoiceOption> ToLegacyChoices(List<BaseUI.ChoiceOption> baseOptions)
    {
        var result = new List<ChoiceOption>();
        if (baseOptions == null)
        {
            return result;
        }

        foreach (BaseUI.ChoiceOption option in baseOptions)
        {
            if (option == null || string.IsNullOrWhiteSpace(option.label))
            {
                continue;
            }

            result.Add(new ChoiceOption
            {
                label = option.label,
                nextNodeId = option.nextNodeId
            });
        }

        return result;
    }

    private static string GetBaseChoiceNextId(BaseUI.ChoiceNode node, int optionIndex)
    {
        if (node?.options == null || node.options.Count == 0)
        {
            return null;
        }

        if (optionIndex < 0 || optionIndex >= node.options.Count)
        {
            optionIndex = 0;
        }

        BaseUI.ChoiceOption selected = node.options[optionIndex];
        if (selected != null && !string.IsNullOrEmpty(selected.nextNodeId))
        {
            return selected.nextNodeId;
        }

        foreach (BaseUI.ChoiceOption option in node.options)
        {
            if (option != null && !string.IsNullOrEmpty(option.nextNodeId))
            {
                return option.nextNodeId;
            }
        }

        return null;
    }

    private static string GetRandomNextId(BaseFlow.RandomNode node)
    {
        if (node?.options == null || node.options.Count == 0)
        {
            return null;
        }

        float totalWeight = 0f;
        foreach (BaseFlow.RandomOption option in node.options)
        {
            if (option == null || string.IsNullOrEmpty(option.nextNodeId))
            {
                continue;
            }

            totalWeight += Mathf.Max(0f, option.weight);
        }

        if (totalWeight <= 0f)
        {
            foreach (BaseFlow.RandomOption option in node.options)
            {
                if (option != null && !string.IsNullOrEmpty(option.nextNodeId))
                {
                    return option.nextNodeId;
                }
            }

            return null;
        }

        float roll = Random.Range(0f, totalWeight);
        float accum = 0f;
        foreach (BaseFlow.RandomOption option in node.options)
        {
            if (option == null || string.IsNullOrEmpty(option.nextNodeId))
            {
                continue;
            }

            accum += Mathf.Max(0f, option.weight);
            if (roll <= accum)
            {
                return option.nextNodeId;
            }
        }

        foreach (BaseFlow.RandomOption option in node.options)
        {
            if (option != null && !string.IsNullOrEmpty(option.nextNodeId))
            {
                return option.nextNodeId;
            }
        }

        return null;
    }

    private bool TryRequestSetGameObjectActive(SetGameObjectActiveNode node)
    {
        if (node == null)
        {
            return false;
        }

        string siteId = node.siteId != null ? node.siteId.Trim() : string.Empty;
        if (string.IsNullOrEmpty(siteId))
        {
            Debug.Log("[SetGameObjectActive] Skipped: siteId is empty.");
            return false;
        }

        if (gameServer == null)
        {
            Debug.Log("[SetGameObjectActive] Skipped: gameServer is missing.");
            return false;
        }

        if (requestManager != null && !requestManager.TryStartRequest("SetGameObjectActive"))
        {
            Debug.Log("[SetGameObjectActive] Skipped: request manager blocked request.");
            return false;
        }

        if (node.isActive)
        {
            string visualId = ResolveVisualId(node);
            if (string.IsNullOrEmpty(visualId))
            {
                Debug.Log("[SetGameObjectActive] Skipped: visualId is empty.");
                requestManager?.FinishRequest();
                return false;
            }

            Debug.Log($"[SetGameObjectActive] START construct siteId='{siteId}' visualId='{visualId}'");
            pendingServerTask = gameServer.TryConstructSiteVisualAsync(siteId, visualId);
        }
        else
        {
            Debug.Log($"[SetGameObjectActive] START remove siteId='{siteId}'");
            pendingServerTask = gameServer.TryRemoveSiteVisualAsync(siteId);
        }

        pendingSuccessNodeId = node.nextNodeId;
        pendingFailNodeId = node.nextNodeId;
        pendingRequestLabel = "SetGameObjectActive";
        return true;
    }

    private static string ResolveVisualId(SetGameObjectActiveNode node)
    {
        if (node == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(node.visualId))
        {
            return node.visualId.Trim();
        }

        return node.targetObject != null ? node.targetObject.name : null;
    }

    private bool IsStealContext()
    {
        return currentContext != null && currentContext.contextType == InteractionContextType.Steal;
    }

    private BusinessQuestNode ResolveStartNode()
    {
        string graphId = GetGraphId();
        if (string.IsNullOrEmpty(graphId))
        {
            Debug.Log("[GraphRunner] Start from beginning (graphId missing)");
            return graph.GetStartNode();
        }

        if (playerStateSync == null || !playerStateSync.TryGetGraphCheckpoint(graphId, out string checkpointId))
        {
            Debug.Log($"[GraphRunner] Start from beginning graphId='{graphId}'");
            return graph.GetStartNode();
        }

        if (string.IsNullOrEmpty(checkpointId))
        {
            Debug.Log($"[GraphRunner] Start from beginning graphId='{graphId}'");
            return graph.GetStartNode();
        }

        CheckpointNode checkpoint = graph.GetCheckpointNodeById(checkpointId);
        if (checkpoint == null)
        {
            Debug.Log($"[GraphRunner] Checkpoint not found graphId='{graphId}' checkpointId='{checkpointId}'");
            return graph.GetStartNode();
        }

        var nextNode = graph.GetNodeById(checkpoint.nextNodeId);
        if (nextNode == null)
        {
            Debug.Log($"[GraphRunner] Checkpoint has no next node graphId='{graphId}' checkpointId='{checkpointId}', start from beginning");
            return graph.GetStartNode();
        }

        Debug.Log($"[GraphRunner] Resume from checkpoint graphId='{graphId}' checkpointId='{checkpointId}'");
        return nextNode;
    }

    private void SaveCheckpoint(CheckpointNode node)
    {
        if (node == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(node.checkpointId))
        {
            return;
        }

        string graphId = GetGraphId();
        if (string.IsNullOrEmpty(graphId))
        {
            return;
        }

        if (gameServer == null)
        {
            Debug.Log("[Checkpoint] Result: Fail - ServerMissing");
            return;
        }

        if (requestManager != null && !requestManager.TryStartRequest("SaveCheckpoint"))
        {
            Debug.Log("[Checkpoint] Result: Fail - RequestBlocked");
            return;
        }

        pendingServerTask = gameServer.TrySaveCheckpointAsync(graphId, node.checkpointId);
        pendingSuccessNodeId = node.nextNodeId;
        pendingFailNodeId = node.nextNodeId;
        pendingRequestLabel = "SaveCheckpoint";
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
                _ = FireAndForgetCheckpointAsync(checkpointNode);
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
        delayPending = false;
        delayUntilTime = 0f;
        delayNextNodeId = null;
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
        string graphId = GetGraphId();
        if (string.IsNullOrEmpty(graphId))
        {
            return;
        }

        if (gameServer == null)
        {
            return;
        }

        _ = ClearCheckpointAsync(graphId);
    }

    private async Task ClearCheckpointAsync(string graphId)
    {
        try
        {
            var result = await gameServer.TrySaveCheckpointAsync(graphId, null);
            if (result?.ProfileSnapshot != null && profileSync != null)
            {
                profileSync.ApplySnapshot(result.ProfileSnapshot);
                Debug.Log("[Checkpoint] Cleared and snapshot applied.");
            }
        }
        catch
        {
            Debug.Log("[Checkpoint] Clear failed - Exception");
        }
    }

    private async Task FireAndForgetCheckpointAsync(CheckpointNode node)
    {
        if (node == null)
        {
            return;
        }

        string graphId = GetGraphId();
        if (string.IsNullOrEmpty(graphId))
        {
            return;
        }

        if (gameServer == null)
        {
            return;
        }

        try
        {
            var result = await gameServer.TrySaveCheckpointAsync(graphId, node.checkpointId);
            if (result?.ProfileSnapshot != null && profileSync != null)
            {
                profileSync.ApplySnapshot(result.ProfileSnapshot);
                Debug.Log("[Checkpoint] Saved and snapshot applied.");
            }
        }
        catch
        {
            Debug.Log("[Checkpoint] Save failed - Exception");
        }
    }
}
