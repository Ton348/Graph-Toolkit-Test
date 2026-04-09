using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using BaseCinematics = GraphCore.BaseNodes.Runtime.Cinematics;
using BaseFlow = GraphCore.BaseNodes.Runtime.Flow;
using BaseServer = GraphCore.BaseNodes.Runtime.Server;
using BaseUI = GraphCore.BaseNodes.Runtime.UI;
using BaseUtility = GraphCore.BaseNodes.Runtime.Utility;
using BaseWorld = GraphCore.BaseNodes.Runtime.World;

public sealed class BaseGraphRunner
{
    private BaseGraph graph;
    private GraphExecutionContext context;
    private BusinessQuestNode currentNode;

    public bool IsRunning { get; private set; }

    public async Task RunAsync(BaseGraph graph, GraphExecutionContext context)
    {
        if (IsRunning || graph == null)
        {
            return;
        }

        this.graph = graph;
        this.context = context ?? new GraphExecutionContext();
        ResolveUiServicesIfNeeded();

        currentNode = graph.GetStartNode();
        IsRunning = true;

        while (IsRunning && currentNode != null)
        {
            await ExecuteNode();
        }

        Stop();
    }

    public void Stop()
    {
        IsRunning = false;
        graph = null;
        context = null;
        currentNode = null;
    }

    private async Task ExecuteNode()
    {
        if (!IsRunning || currentNode == null || graph == null)
        {
            Stop();
            return;
        }

        switch (currentNode)
        {
            case BaseFlow.StartNode node:
            {
                currentNode = graph.GetNodeById(node.nextNodeId);
                return;
            }

            case BaseFlow.FinishNode:
            {
                Stop();
                return;
            }

            case BaseUtility.LogNode node:
            {
                Debug.Log(node.message ?? string.Empty);
                currentNode = graph.GetNodeById(node.nextNodeId);
                return;
            }

            case BaseFlow.DelayNode node:
            {
                if (node.delaySeconds > 0f)
                {
                    int ms = Mathf.Max(1, Mathf.RoundToInt(node.delaySeconds * 1000f));
                    await Task.Delay(ms);
                }

                currentNode = graph.GetNodeById(node.nextNodeId);
                return;
            }

            case BaseFlow.RandomNode node:
            {
                currentNode = graph.GetNodeById(GetRandomNextId(node));
                return;
            }

            case BaseUI.DialogueNode node:
            {
                ResolveUiServicesIfNeeded();
                if (context?.DialogueService != null)
                {
                    await context.DialogueService.ShowAsync(node.dialogueTitle, node.body);
                }
                else
                {
                    Debug.Log($"Dialogue: {node.dialogueTitle}\n{node.body}");
                }

                currentNode = graph.GetNodeById(node.nextNodeId);
                return;
            }

            case BaseUI.ChoiceNode node:
            {
                ResolveUiServicesIfNeeded();

                List<BaseUI.ChoiceOption> validOptions = node.options
                    .Where(o => o != null
                        && !string.IsNullOrWhiteSpace(o.nextNodeId)
                        && !string.IsNullOrWhiteSpace(o.label))
                    .ToList();

                if (validOptions.Count == 0)
                {
                    Stop();
                    return;
                }

                int selectedIndex;
                if (context?.ChoiceService != null)
                {
                    var entries = validOptions
                        .Select(o => new GraphChoiceEntry(o.label ?? string.Empty))
                        .ToList();
                    selectedIndex = await context.ChoiceService.ShowAsync(entries);
                }
                else
                {
                    selectedIndex = 0;
                }

                if (selectedIndex < 0 || selectedIndex >= validOptions.Count)
                {
                    Stop();
                    return;
                }

                BaseUI.ChoiceOption selected = validOptions[selectedIndex];
                currentNode = graph.GetNodeById(selected.nextNodeId);
                return;
            }

            case BaseWorld.MapMarkerNode node:
            {
                if (context?.MapMarkerService != null)
                {
                    context.MapMarkerService.ShowOrUpdateMarker(node.markerId, node.targetObjectName);
                }
                else
                {
                    Debug.Log($"MapMarker: markerId='{node.markerId}', target='{node.targetObjectName}'");
                }

                currentNode = graph.GetNodeById(node.nextNodeId);
                return;
            }

            case BaseCinematics.PlayCutsceneNode node:
            {
                if (context?.CutsceneService != null)
                {
                    await context.CutsceneService.PlayAsync(node.cutsceneReference);
                }
                else
                {
                    Debug.Log($"PlayCutscene: '{node.cutsceneReference}'");
                }

                currentNode = graph.GetNodeById(node.nextNodeId);
                return;
            }

            case BaseServer.CheckpointNode node:
            {
                bool success = false;
                if (context?.CheckpointService != null)
                {
                    success = node.action == BaseServer.CheckpointAction.Clear
                        ? await context.CheckpointService.ClearAsync(node.checkpointId)
                        : await context.CheckpointService.SaveAsync(node.checkpointId);
                }
                else
                {
                    Debug.Log($"Checkpoint fallback: action='{node.action}', checkpointId='{node.checkpointId}'");
                }

                currentNode = graph.GetNodeById(success ? node.successNodeId : node.failNodeId);
                return;
            }

            case BaseServer.StartQuestNode node:
            {
                bool success = false;
                if (context?.QuestService != null)
                {
                    success = await context.QuestService.StartQuestAsync(node.questId);
                }
                else
                {
                    Debug.Log($"StartQuest fallback: questId='{node.questId}'");
                }

                currentNode = graph.GetNodeById(success ? node.successNodeId : node.failNodeId);
                return;
            }

            case BaseServer.CompleteQuestNode node:
            {
                bool success = false;
                if (context?.QuestService != null)
                {
                    success = await context.QuestService.CompleteQuestAsync(node.questId);
                }
                else
                {
                    Debug.Log($"CompleteQuest fallback: questId='{node.questId}'");
                }

                currentNode = graph.GetNodeById(success ? node.successNodeId : node.failNodeId);
                return;
            }

            case BaseServer.QuestStateConditionNode node:
            {
                bool matches = false;
                if (context?.QuestService != null)
                {
                    BaseServer.QuestState currentState = await context.QuestService.GetQuestStateAsync(node.questId);
                    matches = currentState == node.state;
                }
                else
                {
                    Debug.Log($"QuestStateCondition fallback: questId='{node.questId}', expected='{node.state}'");
                }

                currentNode = graph.GetNodeById(matches ? node.trueNodeId : node.falseNodeId);
                return;
            }

            default:
            {
                Stop();
                return;
            }
        }
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

    private void ResolveUiServicesIfNeeded()
    {
        if (context == null)
        {
            return;
        }

        if (context.DialogueService != null && context.ChoiceService != null)
        {
            return;
        }

        MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null)
            {
                continue;
            }

            if (context.DialogueService == null && behaviour is IGraphDialogueService dialogueService)
            {
                context.DialogueService = dialogueService;
            }

            if (context.ChoiceService == null && behaviour is IGraphChoiceService choiceService)
            {
                context.ChoiceService = choiceService;
            }

            if (context.DialogueService != null && context.ChoiceService != null)
            {
                return;
            }
        }
    }
}
