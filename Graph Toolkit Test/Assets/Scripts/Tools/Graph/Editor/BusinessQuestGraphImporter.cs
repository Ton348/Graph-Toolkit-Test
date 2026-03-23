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

        if (runtimeNode is SpendMoneyNode spendMoney)
        {
            spendMoney.successNodeId = GetConnectedNodeIdByOutputIndex(node, 0, idMap);
            spendMoney.failNodeId = GetConnectedNodeIdByOutputIndex(node, 1, idMap);
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
        switch (node)
        {
            case StartNodeModel:
                return new StartNode();
            case GiveQuestNodeModel giveQuestNode:
                return new GiveQuestNode
                {
                    questDefinition = GetOptionValue<QuestDefinition>(giveQuestNode, GiveQuestNodeModel.QUEST_OPTION)
                };
            case AddQuestNodeModel addQuestNode:
                return new AddQuestNode
                {
                    questDefinition = GetOptionValue<QuestDefinition>(addQuestNode, AddQuestNodeModel.QUEST_OPTION)
                };
            case DialogueNodeModel dialogueNode:
                return new DialogueNode
                {
                    title = GetOptionValue<string>(dialogueNode, DialogueNodeModel.TITLE_OPTION),
                    bodyText = GetOptionValue<string>(dialogueNode, DialogueNodeModel.BODY_OPTION)
                };
            case ChoiceNodeModel choiceNode:
                return new ChoiceNode
                {
                    options = BuildChoiceOptions(choiceNode)
                };
            case SkillCheckNodeModel skillCheckNode:
                return new SkillCheckNode
                {
                    skillType = GetOptionValue<SkillType>(skillCheckNode, SkillCheckNodeModel.SKILL_OPTION),
                    requiredValue = GetOptionValue<int>(skillCheckNode, SkillCheckNodeModel.REQUIRED_OPTION)
                };
            case SpendMoneyNodeModel spendMoneyNode:
                return new SpendMoneyNode
                {
                    amount = GetOptionValue<int>(spendMoneyNode, SpendMoneyNodeModel.AMOUNT_OPTION)
                };
            case AddMapMarkerNodeModel addMarkerNode:
                return new AddMapMarkerNode
                {
                    markerId = GetOptionValue<string>(addMarkerNode, AddMapMarkerNodeModel.MARKER_ID_OPTION),
                    title = GetOptionValue<string>(addMarkerNode, AddMapMarkerNodeModel.TITLE_OPTION)
                };
            case GoToPointNodeModel goToPointNode:
                return new GoToPointNode
                {
                    markerId = GetOptionValue<string>(goToPointNode, GoToPointNodeModel.MARKER_ID_OPTION),
                    arrivalDistance = GetOptionValue<float>(goToPointNode, GoToPointNodeModel.ARRIVAL_OPTION)
                };
            case WaitForBuildingPurchasedNodeModel waitPurchasedNode:
                return new WaitForBuildingPurchasedNode
                {
                    buildingId = GetOptionValue<string>(waitPurchasedNode, WaitForBuildingPurchasedNodeModel.BUILDING_ID_OPTION)
                };
            case CompleteQuestNodeModel completeQuestNode:
                return new CompleteQuestNode
                {
                    questId = GetOptionValue<string>(completeQuestNode, CompleteQuestNodeModel.QUEST_ID_OPTION)
                };
            case FailQuestNodeModel failQuestNode:
                return new FailQuestNode
                {
                    questId = GetOptionValue<string>(failQuestNode, FailQuestNodeModel.QUEST_ID_OPTION)
                };
            case WaitForBuildingUpgradedNodeModel waitUpgradedNode:
                return new WaitForBuildingUpgradedNode
                {
                    buildingId = GetOptionValue<string>(waitUpgradedNode, WaitForBuildingUpgradedNodeModel.BUILDING_ID_OPTION)
                };
            case EndNodeModel:
                return new EndNode();
            default:
                return new EndNode();
        }
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
            string nextId = GetConnectedNodeIdByOutputIndex(node, i, idMap);
            choice.options[i].nextNodeId = nextId;
        }
    }

    static List<ChoiceOption> BuildChoiceOptions(ChoiceNodeModel node)
    {
        var options = new List<ChoiceOption>();
        AddChoice(options,
            GetOptionValue<string>(node, ChoiceNodeModel.OPTION1_ID),
            GetOptionValue<string>(node, ChoiceNodeModel.OPTION1_LABEL));
        AddChoice(options,
            GetOptionValue<string>(node, ChoiceNodeModel.OPTION2_ID),
            GetOptionValue<string>(node, ChoiceNodeModel.OPTION2_LABEL));
        AddChoice(options,
            GetOptionValue<string>(node, ChoiceNodeModel.OPTION3_ID),
            GetOptionValue<string>(node, ChoiceNodeModel.OPTION3_LABEL));
        return options;
    }

    static void AddChoice(List<ChoiceOption> options, string optionId, string label)
    {
        if (string.IsNullOrEmpty(optionId) && string.IsNullOrEmpty(label))
        {
            return;
        }

        var option = new ChoiceOption
        {
            optionId = string.IsNullOrEmpty(optionId) ? label : optionId,
            label = string.IsNullOrEmpty(label) ? optionId : label
        };
        options.Add(option);
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
