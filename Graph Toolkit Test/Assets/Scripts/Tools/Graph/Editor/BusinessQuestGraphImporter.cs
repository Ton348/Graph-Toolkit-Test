using System;
using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, BusinessQuestEditorGraph.AssetExtension)]
internal class BusinessQuestGraphImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var graph = GraphDatabase.LoadGraphForImporter<BusinessQuestEditorGraph>(ctx.assetPath);
        if (graph == null)
        {
            Debug.LogError($"Failed to load Business Quest graph asset: {ctx.assetPath}");
            return;
        }

        var startNode = graph.GetNodes().OfType<StartNodeModel>().FirstOrDefault();
        if (startNode == null)
        {
            return;
        }

        var runtimeGraph = ScriptableObject.CreateInstance<BusinessQuestGraph>();

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

        if (runtimeNode is SkillCheckNode skillCheck)
        {
            skillCheck.successNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            skillCheck.failNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        if (runtimeNode is ConditionNode condition)
        {
            condition.trueNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            condition.falseNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        if (runtimeNode is SpendMoneyNode spendMoney)
        {
            spendMoney.successNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            spendMoney.failNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        if (runtimeNode is StealActionNode stealAction)
        {
            stealAction.successNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            stealAction.failNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        if (runtimeNode is BranchByInteractionContextNode branch)
        {
            branch.normalNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            branch.stealNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
            return;
        }

        string nextId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
        runtimeNode.nextNodeId = nextId;
        if (runtimeNode is DialogueNode dialogue)
        {
            dialogue.nextNodeId = nextId;
        }
        if (runtimeNode is AddQuestNode addQuest)
        {
            addQuest.nextNodeId = nextId;
        }
        if (runtimeNode is GiveQuestNode giveQuest)
        {
            giveQuest.nextNodeId = nextId;
        }
        if (runtimeNode is CompleteQuestNode completeQuest)
        {
            completeQuest.nextNodeId = nextId;
        }
        if (runtimeNode is FailQuestNode failQuest)
        {
            failQuest.nextNodeId = nextId;
        }
        if (runtimeNode is AddMapMarkerNode addMarker)
        {
            addMarker.nextNodeId = nextId;
        }
        if (runtimeNode is GoToPointNode goToPoint)
        {
            goToPoint.nextNodeId = nextId;
        }
        if (runtimeNode is WaitForBuildingPurchasedNode waitPurchase)
        {
            waitPurchase.nextNodeId = nextId;
        }
        if (runtimeNode is WaitForBuildingUpgradedNode waitUpgrade)
        {
            waitUpgrade.nextNodeId = nextId;
        }
    }

    static BusinessQuestNode ConvertNode(INode node)
    {
        BusinessQuestNode runtimeNode = null;

        switch (node)
        {
            case StartNodeModel:
                runtimeNode = new StartNode();
                break;
            case GiveQuestNodeModel giveQuestNode:
                runtimeNode = new GiveQuestNode
                {
                    questDefinition = GetOptionValue<QuestDefinition>(giveQuestNode, GiveQuestNodeModel.QUEST_OPTION)
                };
                break;
            case AddQuestNodeModel addQuestNode:
                runtimeNode = new AddQuestNode
                {
                    questDefinition = GetOptionValue<QuestDefinition>(addQuestNode, AddQuestNodeModel.QUEST_OPTION)
                };
                break;
            case DialogueNodeModel dialogueNode:
                runtimeNode = new DialogueNode
                {
                    title = GetOptionValue<string>(dialogueNode, DialogueNodeModel.TITLE_OPTION),
                    bodyText = GetOptionValue<string>(dialogueNode, DialogueNodeModel.BODY_OPTION)
                };
                break;
            case ChoiceNodeModel choiceNode:
                runtimeNode = new ChoiceNode
                {
                    options = BuildChoiceOptions(choiceNode)
                };
                break;
            case SkillCheckNodeModel skillCheckNode:
                runtimeNode = new SkillCheckNode
                {
                    skillType = GetOptionValue<SkillType>(skillCheckNode, SkillCheckNodeModel.SKILL_OPTION),
                    requiredValue = GetOptionValue<int>(skillCheckNode, SkillCheckNodeModel.REQUIRED_OPTION)
                };
                break;
            case SpendMoneyNodeModel spendMoneyNode:
                runtimeNode = new SpendMoneyNode
                {
                    amount = GetOptionValue<int>(spendMoneyNode, SpendMoneyNodeModel.AMOUNT_OPTION)
                };
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
            case WaitForBuildingPurchasedNodeModel waitPurchasedNode:
                runtimeNode = new WaitForBuildingPurchasedNode
                {
                    buildingId = GetOptionValue<string>(waitPurchasedNode, WaitForBuildingPurchasedNodeModel.BUILDING_ID_OPTION)
                };
                break;
            case CompleteQuestNodeModel completeQuestNode:
                runtimeNode = new CompleteQuestNode
                {
                    questId = GetOptionValue<string>(completeQuestNode, CompleteQuestNodeModel.QUEST_ID_OPTION)
                };
                break;
            case FailQuestNodeModel failQuestNode:
                runtimeNode = new FailQuestNode
                {
                    questId = GetOptionValue<string>(failQuestNode, FailQuestNodeModel.QUEST_ID_OPTION)
                };
                break;
            case WaitForBuildingUpgradedNodeModel waitUpgradedNode:
                runtimeNode = new WaitForBuildingUpgradedNode
                {
                    buildingId = GetOptionValue<string>(waitUpgradedNode, WaitForBuildingUpgradedNodeModel.BUILDING_ID_OPTION)
                };
                break;
            case ConditionNodeModel conditionNode:
                runtimeNode = new ConditionNode
                {
                    conditionType = GetOptionValue<ConditionType>(conditionNode, ConditionNodeModel.CONDITION_TYPE_OPTION),
                    targetBuilding = GetOptionValue<BuildingDefinition>(conditionNode, ConditionNodeModel.BUILDING_OPTION),
                    requiredMoney = GetOptionValue<int>(conditionNode, ConditionNodeModel.REQUIRED_MONEY_OPTION),
                    playerStatType = GetOptionValue<PlayerStatType>(conditionNode, ConditionNodeModel.PLAYER_STAT_OPTION),
                    requiredStatValue = GetOptionValue<int>(conditionNode, ConditionNodeModel.REQUIRED_STAT_OPTION),
                    questId = GetOptionValue<string>(conditionNode, ConditionNodeModel.QUEST_ID_OPTION)
                };
                break;
            case WaitForConditionNodeModel waitConditionNode:
                runtimeNode = new WaitForConditionNode
                {
                    conditionType = GetOptionValue<ConditionType>(waitConditionNode, WaitForConditionNodeModel.CONDITION_TYPE_OPTION),
                    targetBuilding = GetOptionValue<BuildingDefinition>(waitConditionNode, WaitForConditionNodeModel.BUILDING_OPTION),
                    requiredMoney = GetOptionValue<int>(waitConditionNode, WaitForConditionNodeModel.REQUIRED_MONEY_OPTION),
                    playerStatType = GetOptionValue<PlayerStatType>(waitConditionNode, WaitForConditionNodeModel.PLAYER_STAT_OPTION),
                    requiredStatValue = GetOptionValue<int>(waitConditionNode, WaitForConditionNodeModel.REQUIRED_STAT_OPTION),
                    questId = GetOptionValue<string>(waitConditionNode, WaitForConditionNodeModel.QUEST_ID_OPTION)
                };
                break;
            case StealActionNodeModel stealActionNode:
                runtimeNode = new StealActionNode
                {
                    stealAmount = GetOptionValue<int>(stealActionNode, StealActionNodeModel.STEAL_AMOUNT_OPTION),
                    canFail = GetOptionValue<bool>(stealActionNode, StealActionNodeModel.CAN_FAIL_OPTION),
                    requiredSpeech = GetOptionValue<int>(stealActionNode, StealActionNodeModel.REQUIRED_SPEECH_OPTION)
                };
                break;
            case RaiseAlertNodeModel raiseAlertNode:
                runtimeNode = new RaiseAlertNode
                {
                    alertMessage = GetOptionValue<string>(raiseAlertNode, RaiseAlertNodeModel.ALERT_MESSAGE_OPTION)
                };
                break;
            case BranchByInteractionContextNodeModel:
                runtimeNode = new BranchByInteractionContextNode();
                break;
            case SetGameObjectActiveNodeModel setActiveNode:
                runtimeNode = new SetGameObjectActiveNode
                {
                    targetObject = GetOptionValue<GameObject>(setActiveNode, SetGameObjectActiveNodeModel.TARGET_OBJECT_OPTION),
                    isActive = GetOptionValue<bool>(setActiveNode, SetGameObjectActiveNodeModel.IS_ACTIVE_OPTION)
                };
                break;
            case EndNodeModel:
                runtimeNode = new EndNode();
                break;
            default:
                runtimeNode = new EndNode();
                break;
        }

        ApplyNodeMetadata(node, runtimeNode);
        return runtimeNode;
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
        SetChoice(options, 0,
            GetOptionValue<string>(node, ChoiceNodeModel.OPTION1_ID),
            GetOptionValue<string>(node, ChoiceNodeModel.OPTION1_LABEL));
        SetChoice(options, 1,
            GetOptionValue<string>(node, ChoiceNodeModel.OPTION2_ID),
            GetOptionValue<string>(node, ChoiceNodeModel.OPTION2_LABEL));
        SetChoice(options, 2,
            GetOptionValue<string>(node, ChoiceNodeModel.OPTION3_ID),
            GetOptionValue<string>(node, ChoiceNodeModel.OPTION3_LABEL));
        SetChoice(options, 3,
            GetOptionValue<string>(node, ChoiceNodeModel.OPTION4_ID),
            GetOptionValue<string>(node, ChoiceNodeModel.OPTION4_LABEL));
        return options;
    }

    static void SetChoice(List<ChoiceOption> options, int index, string optionId, string label)
    {
        if (options == null || index < 0 || index >= options.Count)
        {
            return;
        }

        if (string.IsNullOrEmpty(optionId) && string.IsNullOrEmpty(label))
        {
            options[index] = null;
            return;
        }

        options[index] = new ChoiceOption
        {
            optionId = string.IsNullOrEmpty(optionId) ? label : optionId,
            label = string.IsNullOrEmpty(label) ? optionId : label
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
}
