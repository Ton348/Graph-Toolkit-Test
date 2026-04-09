using System.Collections.Generic;
using System.Linq;
using GraphCore.BaseNodes.Runtime.Server;
using GraphCore.BaseNodes.Runtime.UI;
using GraphCore.BaseNodes.Runtime.World;
using GraphCore.BaseNodes.Runtime.Cinematics;
using GraphCore.BaseNodes.Runtime.Flow;
using GraphCore.BaseNodes.Runtime.Utility;

public static class BaseGraphValidator
{
    public static GraphValidationResult Validate(BaseGraph graph)
    {
        GraphValidationResult result = new GraphValidationResult();

        if (graph == null)
        {
            result.AddError("Graph is null.");
            return result;
        }

        if (graph.nodes == null || graph.nodes.Count == 0)
        {
            result.AddError("Graph contains no nodes.");
            return result;
        }

        Dictionary<string, BaseGraphNode> lookup = graph.BuildNodeLookup(result);
        if (lookup.Count == 0)
        {
            result.AddError("Graph node lookup is empty after build.");
            return result;
        }

        if (string.IsNullOrWhiteSpace(graph.startNodeId))
        {
            result.AddError("Graph startNodeId is empty.");
        }
        else if (!lookup.ContainsKey(graph.startNodeId))
        {
            result.AddError($"Start node '{graph.startNodeId}' does not exist.");
        }

        foreach (BaseGraphNode node in graph.nodes)
        {
            if (node == null)
            {
                continue;
            }

            ValidateBaseNext(lookup, node, node.nextNodeId, result);

            if (node is CheckpointNode checkpointNode)
            {
                ValidateLink(lookup, checkpointNode, "successNodeId", checkpointNode.successNodeId, result);
                ValidateLink(lookup, checkpointNode, "failNodeId", checkpointNode.failNodeId, result);
            }

            if (node is StartQuestNode startQuestNode)
            {
                ValidateLink(lookup, startQuestNode, "successNodeId", startQuestNode.successNodeId, result);
                ValidateLink(lookup, startQuestNode, "failNodeId", startQuestNode.failNodeId, result);
            }

            if (node is CompleteQuestNode completeQuestNode)
            {
                ValidateLink(lookup, completeQuestNode, "successNodeId", completeQuestNode.successNodeId, result);
                ValidateLink(lookup, completeQuestNode, "failNodeId", completeQuestNode.failNodeId, result);
            }

            if (node is QuestStateConditionNode conditionNode)
            {
                ValidateLink(lookup, conditionNode, "trueNodeId", conditionNode.trueNodeId, result);
                ValidateLink(lookup, conditionNode, "falseNodeId", conditionNode.falseNodeId, result);
            }

            if (node is ChoiceNode choiceNode)
            {
                ValidateChoiceOptions(lookup, choiceNode, result);
            }

            if (node is RandomNode randomNode)
            {
                ValidateRandomOptions(lookup, randomNode, result);
            }

            ValidateRequiredFields(node, result);
            ValidateDeadEndNode(node, result);
        }

        ValidateUnreachableNodes(graph, lookup, result);
        return result;
    }

    private static void ValidateBaseNext(Dictionary<string, BaseGraphNode> lookup, BaseGraphNode node, string nextNodeId, GraphValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(nextNodeId))
        {
            return;
        }

        ValidateLink(lookup, node, "nextNodeId", nextNodeId, result);
    }

    private static void ValidateChoiceOptions(Dictionary<string, BaseGraphNode> lookup, ChoiceNode node, GraphValidationResult result)
    {
        if (node.options == null || node.options.Count == 0)
        {
            result.AddWarning($"Choice node '{node.Id}' has no options.", node.Id, node.GetType().Name, nameof(node.options));
            return;
        }

        bool hasValidOption = false;

        for (int i = 0; i < node.options.Count; i++)
        {
            ChoiceOption option = node.options[i];
            if (option == null || string.IsNullOrWhiteSpace(option.nextNodeId))
            {
                continue;
            }

            hasValidOption = true;
            ValidateLink(lookup, node, $"options[{i}].nextNodeId", option.nextNodeId, result);
        }

        if (!hasValidOption)
        {
            result.AddWarning($"Choice node '{node.Id}' has no valid outgoing options.", node.Id, node.GetType().Name, nameof(node.options));
        }
    }

    private static void ValidateRandomOptions(Dictionary<string, BaseGraphNode> lookup, RandomNode node, GraphValidationResult result)
    {
        if (node.options == null || node.options.Count == 0)
        {
            result.AddWarning($"Random node '{node.Id}' has no options.", node.Id, node.GetType().Name, nameof(node.options));
            return;
        }

        bool hasValidOption = false;
        for (int i = 0; i < node.options.Count; i++)
        {
            RandomOption option = node.options[i];
            if (option == null || string.IsNullOrWhiteSpace(option.nextNodeId))
            {
                continue;
            }

            hasValidOption = true;
            ValidateLink(lookup, node, $"options[{i}].nextNodeId", option.nextNodeId, result);
        }

        if (!hasValidOption)
        {
            result.AddWarning($"Random node '{node.Id}' has no valid outgoing options.", node.Id, node.GetType().Name, nameof(node.options));
        }
    }

    private static void ValidateLink(Dictionary<string, BaseGraphNode> lookup, BaseGraphNode sourceNode, string fieldName, string nodeId, GraphValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            return;
        }

        if (!lookup.ContainsKey(nodeId))
        {
            result.AddError($"Node '{sourceNode.Id}' has invalid link '{fieldName}' -> '{nodeId}'.", sourceNode.Id, sourceNode.GetType().Name, fieldName);
            return;
        }

        if (nodeId == sourceNode.Id)
        {
            result.AddWarning($"Node '{sourceNode.Id}' has self-link in '{fieldName}'.", sourceNode.Id, sourceNode.GetType().Name, fieldName);
        }
    }

    private static void ValidateRequiredFields(BaseGraphNode node, GraphValidationResult result)
    {
        if (node is DialogueNode dialogueNode)
        {
            ValidateNotEmpty(dialogueNode.Id, dialogueNode.GetType().Name, nameof(dialogueNode.dialogueTitle), dialogueNode.dialogueTitle, result);
            ValidateNotEmpty(dialogueNode.Id, dialogueNode.GetType().Name, nameof(dialogueNode.body), dialogueNode.body, result);
        }

        if (node is ChoiceNode choiceNode)
        {
            if (choiceNode.options != null)
            {
                bool hasValidLabel = choiceNode.options.Any(option => option != null && !string.IsNullOrWhiteSpace(option.label));
                if (!hasValidLabel)
                {
                    result.AddWarning($"Choice node '{choiceNode.Id}' has no non-empty labels.", choiceNode.Id, choiceNode.GetType().Name, nameof(choiceNode.options));
                }
            }
        }

        if (node is DelayNode delayNode && delayNode.delaySeconds < 0f)
        {
            result.AddWarning($"Delay node '{node.Id}' has negative delay.", node.Id, node.GetType().Name, nameof(delayNode.delaySeconds));
        }

        if (node is LogNode logNode)
        {
            ValidateNotEmpty(logNode.Id, logNode.GetType().Name, nameof(logNode.message), logNode.message, result, false);
        }

        if (node is MapMarkerNode mapMarkerNode)
        {
            ValidateNotEmpty(mapMarkerNode.Id, mapMarkerNode.GetType().Name, nameof(mapMarkerNode.markerId), mapMarkerNode.markerId, result);
            ValidateNotEmpty(mapMarkerNode.Id, mapMarkerNode.GetType().Name, nameof(mapMarkerNode.targetObjectName), mapMarkerNode.targetObjectName, result, false);
        }

        if (node is PlayCutsceneNode playCutsceneNode)
        {
            ValidateNotEmpty(playCutsceneNode.Id, playCutsceneNode.GetType().Name, nameof(playCutsceneNode.cutsceneReference), playCutsceneNode.cutsceneReference, result);
        }

        if (node is CheckpointNode checkpointNode)
        {
            ValidateNotEmpty(checkpointNode.Id, checkpointNode.GetType().Name, nameof(checkpointNode.checkpointId), checkpointNode.checkpointId, result);
        }

        if (node is StartQuestNode startQuestNode)
        {
            ValidateNotEmpty(startQuestNode.Id, startQuestNode.GetType().Name, nameof(startQuestNode.questId), startQuestNode.questId, result);
        }

        if (node is CompleteQuestNode completeQuestNode)
        {
            ValidateNotEmpty(completeQuestNode.Id, completeQuestNode.GetType().Name, nameof(completeQuestNode.questId), completeQuestNode.questId, result);
        }

        if (node is QuestStateConditionNode conditionNode)
        {
            ValidateNotEmpty(conditionNode.Id, conditionNode.GetType().Name, nameof(conditionNode.questId), conditionNode.questId, result);
        }
    }

    private static void ValidateDeadEndNode(BaseGraphNode node, GraphValidationResult result)
    {
        if (node is FinishNode)
        {
            return;
        }

        if (HasOutgoingLinks(node))
        {
            return;
        }

        result.AddWarning($"Node '{node.Id}' has no outgoing links and may terminate graph execution unexpectedly.", node.Id, node.GetType().Name);
    }

    private static bool HasOutgoingLinks(BaseGraphNode node)
    {
        foreach (string nextNodeId in EnumerateOutgoingLinks(node))
        {
            if (!string.IsNullOrWhiteSpace(nextNodeId))
            {
                return true;
            }
        }

        return false;
    }

    private static void ValidateNotEmpty(string nodeId, string nodeType, string fieldName, string value, GraphValidationResult result, bool error = true)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (error)
        {
            result.AddError($"Node '{nodeId}' has empty required field '{fieldName}'.", nodeId, nodeType, fieldName);
        }
        else
        {
            result.AddWarning($"Node '{nodeId}' has empty optional field '{fieldName}'.", nodeId, nodeType, fieldName);
        }
    }

    private static void ValidateUnreachableNodes(BaseGraph graph, Dictionary<string, BaseGraphNode> lookup, GraphValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(graph.startNodeId) || !lookup.ContainsKey(graph.startNodeId))
        {
            return;
        }

        HashSet<string> visited = new HashSet<string>();
        Queue<string> queue = new Queue<string>();
        queue.Enqueue(graph.startNodeId);

        while (queue.Count > 0)
        {
            string nodeId = queue.Dequeue();
            if (!visited.Add(nodeId))
            {
                continue;
            }

            if (!lookup.TryGetValue(nodeId, out BaseGraphNode node) || node == null)
            {
                continue;
            }

            foreach (string nextId in EnumerateOutgoingLinks(node))
            {
                if (string.IsNullOrWhiteSpace(nextId) || visited.Contains(nextId))
                {
                    continue;
                }

                if (lookup.ContainsKey(nextId))
                {
                    queue.Enqueue(nextId);
                }
            }
        }

        for (int i = 0; i < graph.nodes.Count; i++)
        {
            BaseGraphNode node = graph.nodes[i];
            if (node == null || string.IsNullOrWhiteSpace(node.Id))
            {
                continue;
            }

            if (!visited.Contains(node.Id))
            {
                result.AddWarning($"Node '{node.Id}' is unreachable from start node '{graph.startNodeId}'.", node.Id, node.GetType().Name, nodeIndex: i);
            }
        }
    }

    private static IEnumerable<string> EnumerateOutgoingLinks(BaseGraphNode node)
    {
        if (!string.IsNullOrWhiteSpace(node.nextNodeId))
        {
            yield return node.nextNodeId;
        }

        if (node is CheckpointNode checkpointNode)
        {
            yield return checkpointNode.successNodeId;
            yield return checkpointNode.failNodeId;
        }

        if (node is StartQuestNode startQuestNode)
        {
            yield return startQuestNode.successNodeId;
            yield return startQuestNode.failNodeId;
        }

        if (node is CompleteQuestNode completeQuestNode)
        {
            yield return completeQuestNode.successNodeId;
            yield return completeQuestNode.failNodeId;
        }

        if (node is QuestStateConditionNode conditionNode)
        {
            yield return conditionNode.trueNodeId;
            yield return conditionNode.falseNodeId;
        }

        if (node is ChoiceNode choiceNode && choiceNode.options != null)
        {
            for (int i = 0; i < choiceNode.options.Count; i++)
            {
                ChoiceOption option = choiceNode.options[i];
                if (option != null)
                {
                    yield return option.nextNodeId;
                }
            }
        }

        if (node is RandomNode randomNode && randomNode.options != null)
        {
            for (int i = 0; i < randomNode.options.Count; i++)
            {
                RandomOption option = randomNode.options[i];
                if (option != null)
                {
                    yield return option.nextNodeId;
                }
            }
        }
    }
}
