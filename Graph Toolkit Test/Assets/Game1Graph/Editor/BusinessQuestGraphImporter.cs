using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, "bqg")]
internal class BusinessQuestGraphImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var runtimeGraph = ScriptableObject.CreateInstance<BusinessQuestGraph>();

        var graph = GraphDatabase.LoadGraphForImporter<BusinessQuestEditorGraph>(ctx.assetPath);
        if (!(graph is BusinessQuestEditorGraph))
        {
            runtimeGraph.startNodeId = null;
            runtimeGraph.nodes = new List<BusinessQuestNode>();
            ctx.AddObjectToAsset("RuntimeGraph", runtimeGraph);
            ctx.SetMainObject(runtimeGraph);
            return;
        }

        if (graph == null)
        {
            runtimeGraph.startNodeId = null;
            runtimeGraph.nodes = new List<BusinessQuestNode>();
            ctx.AddObjectToAsset("RuntimeGraph", runtimeGraph);
            ctx.SetMainObject(runtimeGraph);
            return;
        }

        var startNode = graph.GetNodes().FirstOrDefault(n =>
            n is StartNodeModel ||
            IsNodeType(n, "GraphCore.BaseNodes.Editor.Flow.StartNodeModel"));
        if (startNode == null)
        {
            runtimeGraph.startNodeId = null;
            runtimeGraph.nodes = new List<BusinessQuestNode>();
            ctx.AddObjectToAsset("RuntimeGraph", runtimeGraph);
            ctx.SetMainObject(runtimeGraph);
            return;
        }

        var runtimeNodes = new List<BusinessQuestNode>();
        var nodeMap = new Dictionary<INode, BusinessQuestNode>();
        var idMap = new Dictionary<INode, string>();

        foreach (INode node in graph.GetNodes())
        {
            BusinessQuestNode runtimeNode = ConvertNode(node);
            if (runtimeNode == null)
            {
                continue;
            }

            runtimeNode.id = Guid.NewGuid().ToString();
            runtimeNodes.Add(runtimeNode);
            nodeMap[node] = runtimeNode;
            idMap[node] = runtimeNode.id;
        }

        foreach (var pair in nodeMap)
        {
            ApplyConnections(pair.Key, pair.Value, idMap);
        }

        runtimeGraph.startNodeId = idMap.TryGetValue(startNode, out string startId) ? startId : null;
        runtimeGraph.nodes = runtimeNodes;

        ctx.AddObjectToAsset("RuntimeGraph", runtimeGraph);
        ctx.SetMainObject(runtimeGraph);
    }

    static void ApplyConnections(INode node, BusinessQuestNode runtimeNode, Dictionary<INode, string> idMap)
    {
        if (node == null || runtimeNode == null)
        {
            return;
        }

        if (runtimeNode is ChoiceNode choice)
        {
            ApplyChoiceConnections(node, choice, idMap);
            return;
        }

        if (IsRuntimeType(runtimeNode, "GraphCore.BaseNodes.Runtime.UI.ChoiceNode"))
        {
            ApplyBaseChoiceConnections(node, runtimeNode, idMap);
            return;
        }

        if (IsRuntimeType(runtimeNode, "GraphCore.BaseNodes.Runtime.Flow.RandomNode"))
        {
            ApplyBaseRandomConnections(node, runtimeNode, idMap);
            return;
        }

        if (runtimeNode is ConditionNode condition)
        {
            condition.trueNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            condition.falseNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        if (runtimeNode is RefreshProfileNode refreshProfile)
        {
            refreshProfile.successNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            refreshProfile.failNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }


        if (runtimeNode is RequestBuyBuildingNode requestBuy)
        {
            requestBuy.successNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            requestBuy.failNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        if (runtimeNode is RequestTradeOfferNode requestTrade)
        {
            requestTrade.successNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            requestTrade.failNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        if (runtimeNode is RequestStartQuestNode requestStartQuest)
        {
            requestStartQuest.successNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            requestStartQuest.failNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        if (runtimeNode is RequestCompleteQuestNode requestCompleteQuest)
        {
            requestCompleteQuest.successNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            requestCompleteQuest.failNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        if (runtimeNode is RequestRentBusinessNode requestRentBusiness)
        {
            requestRentBusiness.successNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            requestRentBusiness.failNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        if (runtimeNode is RequestAssignBusinessTypeNode requestAssignBusinessType)
        {
            requestAssignBusinessType.successNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            requestAssignBusinessType.failNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        if (runtimeNode is RequestInstallBusinessModuleNode requestInstallBusinessModule)
        {
            requestInstallBusinessModule.successNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            requestInstallBusinessModule.failNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        if (runtimeNode is RequestAssignSupplierNode requestAssignSupplier)
        {
            requestAssignSupplier.successNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            requestAssignSupplier.failNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        if (runtimeNode is RequestHireBusinessWorkerNode requestHireBusinessWorker)
        {
            requestHireBusinessWorker.successNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            requestHireBusinessWorker.failNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        if (runtimeNode is RequestSetBusinessOpenNode requestSetBusinessOpen)
        {
            requestSetBusinessOpen.successNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            requestSetBusinessOpen.failNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        if (runtimeNode is RequestOpenBusinessNode requestOpenBusiness)
        {
            requestOpenBusiness.successNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            requestOpenBusiness.failNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        if (runtimeNode is RequestCloseBusinessNode requestCloseBusiness)
        {
            requestCloseBusiness.successNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            requestCloseBusiness.failNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        if (runtimeNode is RequestSetBusinessMarkupNode requestSetBusinessMarkup)
        {
            requestSetBusinessMarkup.successNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            requestSetBusinessMarkup.failNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        if (runtimeNode is RequestUnlockContactNode requestUnlockContact)
        {
            requestUnlockContact.successNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            requestUnlockContact.failNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        if (runtimeNode is CheckBusinessExistsNode checkBusinessExists)
        {
            checkBusinessExists.trueNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            checkBusinessExists.falseNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        if (runtimeNode is CheckBusinessOpenNode checkBusinessOpen)
        {
            checkBusinessOpen.trueNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            checkBusinessOpen.falseNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        if (runtimeNode is CheckBusinessModuleInstalledNode checkBusinessModuleInstalled)
        {
            checkBusinessModuleInstalled.trueNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            checkBusinessModuleInstalled.falseNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        if (runtimeNode is CheckContactKnownNode checkContactKnown)
        {
            checkContactKnown.trueNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            checkContactKnown.falseNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }



        string nextId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
        runtimeNode.nextNodeId = nextId;
        if (runtimeNode is DialogueNode dialogue)
        {
            dialogue.nextNodeId = nextId;
        }
        if (runtimeNode is AddMapMarkerNode addMarker)
        {
            addMarker.nextNodeId = nextId;
        }
        if (runtimeNode is GoToPointNode goToPoint)
        {
            goToPoint.nextNodeId = nextId;
        }
    }

    static BusinessQuestNode ConvertNode(INode node)
    {
        if (TryConvertBaseNode(node, out BusinessQuestNode baseRuntimeNode))
        {
            ApplyNodeMetadata(node, baseRuntimeNode);
            return baseRuntimeNode;
        }

        BusinessQuestNode runtimeNode = null;

        switch (node)
        {
            case StartNodeModel:
                runtimeNode = new StartNode();
                break;
            case DialogueNodeModel dialogueNode:
                runtimeNode = new DialogueNode
                {
                    title = GetOptionValue<string>(dialogueNode, DialogueNodeModel.TITLE_OPTION),
                    bodyText = GetOptionValue<string>(dialogueNode, DialogueNodeModel.BODY_OPTION),
                    screenshot = GetOptionValue<Sprite>(dialogueNode, DialogueNodeModel.SCREENSHOT_OPTION)
                };
                break;
            case ChoiceNodeModel choiceNode:
                runtimeNode = new ChoiceNode
                {
                    options = BuildChoiceOptions(choiceNode)
                };
                break;
            case RequestBuyBuildingNodeModel requestBuyBuildingNode:
                runtimeNode = new RequestBuyBuildingNode
                {
                    buildingId = GetOptionValue<string>(requestBuyBuildingNode, RequestBuyBuildingNodeModel.BUILDING_ID_OPTION),
                    questAction = GetOptionValue<QuestActionType>(requestBuyBuildingNode, RequestBuyBuildingNodeModel.QUEST_ACTION_OPTION),
                    questId = GetOptionValue<string>(requestBuyBuildingNode, RequestBuyBuildingNodeModel.QUEST_ID_OPTION)
                };
                break;
            case RequestTradeOfferNodeModel requestTradeOfferNode:
                runtimeNode = new RequestTradeOfferNode
                {
                    buildingId = GetOptionValue<string>(requestTradeOfferNode, RequestTradeOfferNodeModel.BUILDING_ID_OPTION)
                };
                break;
            case RequestStartQuestNodeModel requestStartQuestNode:
                runtimeNode = new RequestStartQuestNode
                {
                    questId = GetOptionValue<string>(requestStartQuestNode, RequestStartQuestNodeModel.QUEST_ID_OPTION)
                };
                break;
            case RequestCompleteQuestNodeModel requestCompleteQuestNode:
                runtimeNode = new RequestCompleteQuestNode
                {
                    questId = GetOptionValue<string>(requestCompleteQuestNode, RequestCompleteQuestNodeModel.QUEST_ID_OPTION)
                };
                break;
            case RequestRentBusinessNodeModel requestRentBusinessNode:
                runtimeNode = new RequestRentBusinessNode
                {
                    lotId = GetOptionValue<string>(requestRentBusinessNode, RequestRentBusinessNodeModel.LOT_ID_OPTION)
                };
                break;
            case RequestAssignBusinessTypeNodeModel requestAssignBusinessTypeNode:
                runtimeNode = new RequestAssignBusinessTypeNode
                {
                    lotId = GetOptionValue<string>(requestAssignBusinessTypeNode, RequestAssignBusinessTypeNodeModel.LOT_ID_OPTION),
                    businessTypeId = GetOptionValue<string>(requestAssignBusinessTypeNode, RequestAssignBusinessTypeNodeModel.BUSINESS_TYPE_ID_OPTION)
                };
                break;
            case RequestInstallBusinessModuleNodeModel requestInstallBusinessModuleNode:
                runtimeNode = new RequestInstallBusinessModuleNode
                {
                    lotId = GetOptionValue<string>(requestInstallBusinessModuleNode, RequestInstallBusinessModuleNodeModel.LOT_ID_OPTION),
                    moduleId = GetOptionValue<string>(requestInstallBusinessModuleNode, RequestInstallBusinessModuleNodeModel.MODULE_ID_OPTION)
                };
                break;
            case RequestAssignSupplierNodeModel requestAssignSupplierNode:
                runtimeNode = new RequestAssignSupplierNode
                {
                    lotId = GetOptionValue<string>(requestAssignSupplierNode, RequestAssignSupplierNodeModel.LOT_ID_OPTION),
                    supplierId = GetOptionValue<string>(requestAssignSupplierNode, RequestAssignSupplierNodeModel.SUPPLIER_ID_OPTION)
                };
                break;
            case RequestHireBusinessWorkerNodeModel requestHireBusinessWorkerNode:
                runtimeNode = new RequestHireBusinessWorkerNode
                {
                    lotId = GetOptionValue<string>(requestHireBusinessWorkerNode, RequestHireBusinessWorkerNodeModel.LOT_ID_OPTION),
                    roleId = GetOptionValue<string>(requestHireBusinessWorkerNode, RequestHireBusinessWorkerNodeModel.ROLE_ID_OPTION),
                    contactId = GetOptionValue<string>(requestHireBusinessWorkerNode, RequestHireBusinessWorkerNodeModel.CONTACT_ID_OPTION)
                };
                break;
            case RequestSetBusinessOpenNodeModel requestSetBusinessOpenNode:
                var openAction = GetOptionValue<BusinessOpenAction>(requestSetBusinessOpenNode, RequestSetBusinessOpenNodeModel.ACTION_OPTION);
                runtimeNode = new RequestSetBusinessOpenNode
                {
                    lotId = GetOptionValue<string>(requestSetBusinessOpenNode, RequestSetBusinessOpenNodeModel.LOT_ID_OPTION),
                    open = openAction == BusinessOpenAction.Open
                };
                break;
            case RequestOpenBusinessNodeModel requestOpenBusinessNode:
                runtimeNode = new RequestOpenBusinessNode
                {
                    lotId = GetOptionValue<string>(requestOpenBusinessNode, RequestOpenBusinessNodeModel.LOT_ID_OPTION)
                };
                break;
            case RequestCloseBusinessNodeModel requestCloseBusinessNode:
                runtimeNode = new RequestCloseBusinessNode
                {
                    lotId = GetOptionValue<string>(requestCloseBusinessNode, RequestCloseBusinessNodeModel.LOT_ID_OPTION)
                };
                break;
            case RequestSetBusinessMarkupNodeModel requestSetBusinessMarkupNode:
                runtimeNode = new RequestSetBusinessMarkupNode
                {
                    lotId = GetOptionValue<string>(requestSetBusinessMarkupNode, RequestSetBusinessMarkupNodeModel.LOT_ID_OPTION),
                    markupPercent = GetOptionValue<int>(requestSetBusinessMarkupNode, RequestSetBusinessMarkupNodeModel.MARKUP_OPTION)
                };
                break;
            case RequestUnlockContactNodeModel requestUnlockContactNode:
                runtimeNode = new RequestUnlockContactNode
                {
                    contactId = GetOptionValue<string>(requestUnlockContactNode, RequestUnlockContactNodeModel.CONTACT_ID_OPTION)
                };
                break;
            case CheckBusinessExistsNodeModel checkBusinessExistsNode:
                runtimeNode = new CheckBusinessExistsNode
                {
                    lotId = GetOptionValue<string>(checkBusinessExistsNode, CheckBusinessExistsNodeModel.LOT_ID_OPTION)
                };
                break;
            case CheckBusinessOpenNodeModel checkBusinessOpenNode:
                runtimeNode = new CheckBusinessOpenNode
                {
                    lotId = GetOptionValue<string>(checkBusinessOpenNode, CheckBusinessOpenNodeModel.LOT_ID_OPTION)
                };
                break;
            case CheckBusinessModuleInstalledNodeModel checkBusinessModuleInstalledNode:
                runtimeNode = new CheckBusinessModuleInstalledNode
                {
                    lotId = GetOptionValue<string>(checkBusinessModuleInstalledNode, CheckBusinessModuleInstalledNodeModel.LOT_ID_OPTION),
                    moduleId = GetOptionValue<string>(checkBusinessModuleInstalledNode, CheckBusinessModuleInstalledNodeModel.MODULE_ID_OPTION)
                };
                break;
            case CheckContactKnownNodeModel checkContactKnownNode:
                runtimeNode = new CheckContactKnownNode
                {
                    contactId = GetOptionValue<string>(checkContactKnownNode, CheckContactKnownNodeModel.CONTACT_ID_OPTION)
                };
                break;
            case INode refreshProfileNode when refreshProfileNode.GetType().Name == "RefreshProfileNodeModel":
                runtimeNode = new RefreshProfileNode();
                break;
            case AddMapMarkerNodeModel addMarkerNode:
                runtimeNode = new AddMapMarkerNode
                {
                    markerId = GetOptionValue<string>(addMarkerNode, AddMapMarkerNodeModel.MARKER_ID_OPTION),
                    title = GetOptionValue<string>(addMarkerNode, AddMapMarkerNodeModel.TITLE_OPTION)
                };
                break;
            case GoToPointNodeModel goToPointNode:
                runtimeNode = new GoToPointNode
                {
                    markerId = GetOptionValue<string>(goToPointNode, GoToPointNodeModel.MARKER_ID_OPTION),
                    arrivalDistance = GetOptionValue<float>(goToPointNode, GoToPointNodeModel.ARRIVAL_OPTION)
                };
                break;
            case ConditionNodeModel conditionNode:
                runtimeNode = new ConditionNode
                {
                    conditionType = GetOptionValue<ConditionType>(conditionNode, ConditionNodeModel.CONDITION_TYPE_OPTION),
                    buildingId = GetOptionValue<string>(conditionNode, ConditionNodeModel.BUILDING_OPTION),
                    requiredMoney = GetOptionValue<int>(conditionNode, ConditionNodeModel.REQUIRED_MONEY_OPTION),
                    playerStatType = GetOptionValue<PlayerStatType>(conditionNode, ConditionNodeModel.PLAYER_STAT_OPTION),
                    requiredStatValue = GetOptionValue<int>(conditionNode, ConditionNodeModel.REQUIRED_STAT_OPTION),
                    questId = GetOptionValue<string>(conditionNode, ConditionNodeModel.QUEST_ID_OPTION)
                };
                break;
            case CheckpointNodeModel checkpointNode:
                runtimeNode = new CheckpointNode
                {
                    checkpointId = GetOptionValue<string>(checkpointNode, CheckpointNodeModel.CHECKPOINT_ID_OPTION)
                };
                break;
            case SetGameObjectActiveNodeModel setActiveNode:
                runtimeNode = new SetGameObjectActiveNode
                {
                    targetObject = GetOptionValue<GameObject>(setActiveNode, SetGameObjectActiveNodeModel.TARGET_OBJECT_OPTION),
                    siteId = GetOptionValue<string>(setActiveNode, SetGameObjectActiveNodeModel.SITE_ID_OPTION)
                        ?? GetOptionValue<string>(setActiveNode, SetGameObjectActiveNodeModel.LOT_ID_OPTION),
                    visualId = GetOptionValue<string>(setActiveNode, SetGameObjectActiveNodeModel.VISUAL_ID_OPTION)
                        ?? GetOptionValue<string>(setActiveNode, SetGameObjectActiveNodeModel.LEGACY_SPAWN_KEY_OPTION),
                    isActive = GetOptionValue<bool>(setActiveNode, SetGameObjectActiveNodeModel.IS_ACTIVE_OPTION),
                };
                break;
            case EndNodeModel endNodeModel:
                bool clearCheckpoint = true;
                if (node is Node endNode && TryGetOptionValue(endNode, EndNodeModel.CLEAR_CHECKPOINT_OPTION, out bool clearValue))
                {
                    clearCheckpoint = clearValue;
                }
                runtimeNode = new EndNode
                {
                    clearCheckpoint = clearCheckpoint,
                    completeQuestId = GetOptionValue<string>(endNodeModel, EndNodeModel.COMPLETE_QUEST_ID_OPTION)
                };
                break;
            default:
                runtimeNode = new EndNode();
                break;
        }

        ApplyNodeMetadata(node, runtimeNode);
        return runtimeNode;
    }

    static bool TryConvertBaseNode(INode node, out BusinessQuestNode runtimeNode)
    {
        runtimeNode = null;
        if (node is not Node typedNode)
        {
            return false;
        }

        if (IsNodeType(node, "GraphCore.BaseNodes.Editor.Flow.StartNodeModel"))
        {
            runtimeNode = CreateRuntimeNode("GraphCore.BaseNodes.Runtime.Flow.StartNode");
            return runtimeNode != null;
        }

        if (IsNodeType(node, "GraphCore.BaseNodes.Editor.Flow.FinishNodeModel"))
        {
            runtimeNode = CreateRuntimeNode("GraphCore.BaseNodes.Runtime.Flow.FinishNode");
            return runtimeNode != null;
        }

        if (IsNodeType(node, "GraphCore.BaseNodes.Editor.UI.DialogueNodeModel"))
        {
            runtimeNode = CreateRuntimeNode("GraphCore.BaseNodes.Runtime.UI.DialogueNode");
            if (runtimeNode == null)
            {
                return false;
            }

            SetFieldValue(runtimeNode, "dialogueTitle", GetOptionValue<string>(typedNode, "Title"));
            SetFieldValue(runtimeNode, "body", GetOptionValue<string>(typedNode, "Body"));
            return true;
        }

        if (IsNodeType(node, "GraphCore.BaseNodes.Editor.UI.ChoiceNodeModel"))
        {
            runtimeNode = CreateRuntimeNode("GraphCore.BaseNodes.Runtime.UI.ChoiceNode");
            if (runtimeNode == null)
            {
                return false;
            }

            BuildBaseChoiceOptions(runtimeNode, typedNode);
            return true;
        }

        if (IsNodeType(node, "GraphCore.BaseNodes.Editor.Flow.DelayNodeModel"))
        {
            runtimeNode = CreateRuntimeNode("GraphCore.BaseNodes.Runtime.Flow.DelayNode");
            if (runtimeNode == null)
            {
                return false;
            }

            SetFieldValue(runtimeNode, "delaySeconds", GetOptionValue<float>(typedNode, "DelaySeconds"));
            return true;
        }

        if (IsNodeType(node, "GraphCore.BaseNodes.Editor.Flow.RandomNodeModel"))
        {
            runtimeNode = CreateRuntimeNode("GraphCore.BaseNodes.Runtime.Flow.RandomNode");
            if (runtimeNode == null)
            {
                return false;
            }

            BuildBaseRandomOptions(runtimeNode, typedNode);
            return true;
        }

        if (IsNodeType(node, "GraphCore.BaseNodes.Editor.Utility.LogNodeModel"))
        {
            runtimeNode = CreateRuntimeNode("GraphCore.BaseNodes.Runtime.Utility.LogNode");
            if (runtimeNode == null)
            {
                return false;
            }

            SetFieldValue(runtimeNode, "message", GetOptionValue<string>(typedNode, "Message"));
            return true;
        }

        return false;
    }

    static void ApplyNodeMetadata(INode editorNode, BusinessQuestNode runtimeNode)
    {
        if (editorNode is not Node node || runtimeNode == null)
        {
            return;
        }

        if (TryGetOptionValue(node, BusinessQuestEditorNode.TITLE_OPTION, out string title))
        {
            runtimeNode.Title = string.IsNullOrWhiteSpace(title) ? runtimeNode.GetType().Name : title;
        }
        else if (string.IsNullOrWhiteSpace(runtimeNode.Title))
        {
            runtimeNode.Title = runtimeNode.GetType().Name;
        }

        if (TryGetOptionValue(node, BusinessQuestEditorNode.DESCRIPTION_OPTION, out string description))
        {
            runtimeNode.Description = description;
        }

        if (TryGetOptionValue(node, BusinessQuestEditorNode.COMMENT_OPTION, out string comment))
        {
            runtimeNode.Comment = comment;
        }
    }

    static bool TryGetOptionValue<T>(Node node, string optionName, out T value)
    {
        var option = node.GetNodeOptionByName(optionName);
        if (option != null && option.TryGetValue(out value))
        {
            return true;
        }

        value = default;
        return false;
    }

    static string GetConnectedNodeIdByOutputIndex(INode node, int outputIndex, Dictionary<INode, string> idMap)
    {
        if (node == null || outputIndex < 0 || outputIndex >= node.outputPortCount)
        {
            return null;
        }

        var outputPort = node.GetOutputPort(outputIndex);
        var nextPort = outputPort?.firstConnectedPort;
        var nextNode = nextPort?.GetNode();
        if (nextNode == null)
        {
            return null;
        }

        return idMap.TryGetValue(nextNode, out string id) ? id : null;
    }

    static void ApplyChoiceConnections(INode node, ChoiceNode choice, Dictionary<INode, string> idMap)
    {
        if (choice == null || choice.options == null)
        {
            return;
        }

        for (int i = 0; i < choice.options.Count; i++)
        {
            if (choice.options[i] == null)
            {
                continue;
            }

            string nextId = GetConnectedNodeIdByOutputIndex(node, i, idMap);
            choice.options[i].nextNodeId = nextId;
        }
    }

    static List<ChoiceOption> BuildChoiceOptions(ChoiceNodeModel node)
    {
        var options = new List<ChoiceOption> { null, null, null, null };
        SetChoice(options, 0, GetOptionValue<string>(node, ChoiceNodeModel.OPTION1_LABEL));
        SetChoice(options, 1, GetOptionValue<string>(node, ChoiceNodeModel.OPTION2_LABEL));
        SetChoice(options, 2, GetOptionValue<string>(node, ChoiceNodeModel.OPTION3_LABEL));
        SetChoice(options, 3, GetOptionValue<string>(node, ChoiceNodeModel.OPTION4_LABEL));
        return options;
    }

    static void SetChoice(List<ChoiceOption> options, int index, string label)
    {
        if (options == null || index < 0 || index >= options.Count)
        {
            return;
        }

        if (string.IsNullOrEmpty(label))
        {
            options[index] = null;
            return;
        }

        options[index] = new ChoiceOption
        {
            label = label
        };
    }

    static T GetOptionValue<T>(Node node, string optionName)
    {
        var option = node.GetNodeOptionByName(optionName);
        if (option != null && option.TryGetValue(out T value))
        {
            return value;
        }

        return default;
    }

    static bool IsNodeType(INode node, string fullName)
    {
        return node?.GetType().FullName == fullName;
    }

    static bool IsRuntimeType(BusinessQuestNode node, string fullName)
    {
        return node?.GetType().FullName == fullName;
    }

    static BusinessQuestNode CreateRuntimeNode(string fullName)
    {
        Type type = FindType(fullName);
        if (type == null || !typeof(BusinessQuestNode).IsAssignableFrom(type))
        {
            return null;
        }

        return Activator.CreateInstance(type) as BusinessQuestNode;
    }

    static Type FindType(string fullName)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(fullName, false);
            if (type != null)
            {
                return type;
            }
        }

        return null;
    }

    static void SetFieldValue(object target, string fieldName, object value)
    {
        if (target == null || string.IsNullOrEmpty(fieldName))
        {
            return;
        }

        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null)
        {
            return;
        }

        field.SetValue(target, value);
    }

    static IList<object> GetOptionsList(BusinessQuestNode node)
    {
        if (node == null)
        {
            return null;
        }

        FieldInfo field = node.GetType().GetField("options", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null)
        {
            return null;
        }

        object rawList = field.GetValue(node);
        if (rawList is System.Collections.IEnumerable enumerable)
        {
            var list = new List<object>();
            foreach (object item in enumerable)
            {
                list.Add(item);
            }

            return list;
        }

        return null;
    }

    static void ApplyBaseChoiceConnections(INode node, BusinessQuestNode runtimeNode, Dictionary<INode, string> idMap)
    {
        IList<object> options = GetOptionsList(runtimeNode);
        if (options == null)
        {
            return;
        }

        for (int i = 0; i < options.Count; i++)
        {
            object option = options[i];
            if (option == null)
            {
                continue;
            }

            SetFieldValue(option, "nextNodeId", GetConnectedNodeIdByOutputIndex(node, i, idMap));
        }
    }

    static void ApplyBaseRandomConnections(INode node, BusinessQuestNode runtimeNode, Dictionary<INode, string> idMap)
    {
        IList<object> options = GetOptionsList(runtimeNode);
        if (options == null)
        {
            return;
        }

        for (int i = 0; i < options.Count; i++)
        {
            object option = options[i];
            if (option == null)
            {
                continue;
            }

            SetFieldValue(option, "nextNodeId", GetConnectedNodeIdByOutputIndex(node, i, idMap));
        }
    }

    static void BuildBaseChoiceOptions(BusinessQuestNode runtimeNode, Node modelNode)
    {
        IList<object> options = GetOptionsList(runtimeNode);
        if (options == null || options.Count == 0)
        {
            return;
        }

        string[] labels =
        {
            GetOptionValue<string>(modelNode, "Option1"),
            GetOptionValue<string>(modelNode, "Option2"),
            GetOptionValue<string>(modelNode, "Option3"),
            GetOptionValue<string>(modelNode, "Option4")
        };

        for (int i = 0; i < options.Count && i < labels.Length; i++)
        {
            object option = options[i];
            if (option == null)
            {
                continue;
            }

            SetFieldValue(option, "label", labels[i]);
        }
    }

    static void BuildBaseRandomOptions(BusinessQuestNode runtimeNode, Node modelNode)
    {
        IList<object> options = GetOptionsList(runtimeNode);
        if (options == null || options.Count == 0)
        {
            return;
        }

        float[] weights =
        {
            GetOptionValue<float>(modelNode, "Weight1"),
            GetOptionValue<float>(modelNode, "Weight2"),
            GetOptionValue<float>(modelNode, "Weight3"),
            GetOptionValue<float>(modelNode, "Weight4")
        };

        for (int i = 0; i < options.Count && i < weights.Length; i++)
        {
            object option = options[i];
            if (option == null)
            {
                continue;
            }

            SetFieldValue(option, "weight", Mathf.Max(0f, weights[i]));
        }
    }
}
