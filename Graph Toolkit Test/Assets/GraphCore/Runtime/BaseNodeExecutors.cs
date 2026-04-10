using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.BaseNodes.Runtime.Cinematics;
using GraphCore.BaseNodes.Runtime.Flow;
using GraphCore.BaseNodes.Runtime.Server;
using GraphCore.BaseNodes.Runtime.UI;
using GraphCore.BaseNodes.Runtime.Utility;
using GraphCore.BaseNodes.Runtime.World;
using UnityEngine;

public static class BaseNodeExecutorConstants
{
    public const string LogPrefix = "[BaseGraph]";
}

public sealed class StartNodeExecutor : GraphNodeExecutor<StartNode>
{
    protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(StartNode node, GraphExecutionContext context, CancellationToken cancellationToken)
    {
        return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
    }
}

public sealed class FinishNodeExecutor : GraphNodeExecutor<FinishNode>
{
    protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(FinishNode node, GraphExecutionContext context, CancellationToken cancellationToken)
    {
        context?.DialogueService?.EndConversation();
        return UniTask.FromResult(GraphNodeExecutionResult.Stop("Finish node reached."));
    }
}

public sealed class LogNodeExecutor : GraphNodeExecutor<LogNode>
{
    protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(LogNode node, GraphExecutionContext context, CancellationToken cancellationToken)
    {
        Debug.Log(node.message ?? string.Empty);
        return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
    }
}

public sealed class DelayNodeExecutor : GraphNodeExecutor<DelayNode>
{
    protected override async UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(DelayNode node, GraphExecutionContext context, CancellationToken cancellationToken)
    {
        if (node.delaySeconds <= 0f)
        {
            return GraphNodeExecutionResult.ContinueTo(node.nextNodeId);
        }

        int delayMs = Mathf.Max(1, Mathf.RoundToInt(node.delaySeconds * 1000f));
        await UniTask.Delay(delayMs, cancellationToken: cancellationToken);
        return GraphNodeExecutionResult.ContinueTo(node.nextNodeId);
    }
}

public sealed class RandomNodeExecutor : GraphNodeExecutor<RandomNode>
{
    protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(RandomNode node, GraphExecutionContext context, CancellationToken cancellationToken)
    {
        if (node.options == null || node.options.Count == 0)
        {
            return UniTask.FromResult(GraphNodeExecutionResult.Fault($"RandomNode '{node.Id}' has no options.", GraphNodeExecutionErrorType.InvalidNode));
        }

        List<RandomOption> validOptions = new List<RandomOption>();
        for (int i = 0; i < node.options.Count; i++)
        {
            RandomOption option = node.options[i];
            if (option != null && !string.IsNullOrWhiteSpace(option.nextNodeId))
            {
                validOptions.Add(option);
            }
        }

        if (validOptions.Count == 0)
        {
            return UniTask.FromResult(GraphNodeExecutionResult.Fault($"RandomNode '{node.Id}' has no valid options with nextNodeId.", GraphNodeExecutionErrorType.InvalidTransition));
        }

        float totalWeight = 0f;
        for (int i = 0; i < validOptions.Count; i++)
        {
            totalWeight += Mathf.Max(0f, validOptions[i].weight);
        }
        if (totalWeight <= 0f)
        {
            return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(validOptions[0].nextNodeId));
        }

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;
        foreach (RandomOption option in validOptions)
        {
            cumulative += Mathf.Max(0f, option.weight);
            if (roll <= cumulative)
            {
                return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(option.nextNodeId));
            }
        }

        return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(validOptions[validOptions.Count - 1].nextNodeId));
    }
}

public sealed class DialogueNodeExecutor : GraphNodeExecutor<DialogueNode>
{
    protected override async UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(DialogueNode node, GraphExecutionContext context, CancellationToken cancellationToken)
    {
        IGraphDialogueService dialogueService = context.DialogueService;
        if (dialogueService == null)
        {
            Debug.Log($"{BaseNodeExecutorConstants.LogPrefix} Dialogue fallback: {node.dialogueTitle}\n{node.body}");
            Debug.LogWarning($"{BaseNodeExecutorConstants.LogPrefix} Dialogue service is not registered. Using fallback continuation for node '{node.Id}'.");
            return GraphNodeExecutionResult.ContinueTo(node.nextNodeId);
        }

        if (!TryGetImmediateChoiceNode(node, context, out ChoiceNode choiceNode))
        {
            await dialogueService.ShowAsync(node.dialogueTitle, node.body, cancellationToken);
            return GraphNodeExecutionResult.ContinueTo(node.nextNodeId);
        }

        _ = dialogueService.ShowAsync(node.dialogueTitle, node.body, cancellationToken);
        return await ExecuteImmediateChoiceAsync(choiceNode, context, cancellationToken);
    }

    private static bool TryGetImmediateChoiceNode(DialogueNode node, GraphExecutionContext context, out ChoiceNode choiceNode)
    {
        choiceNode = null;
        if (!IsImmediateChoiceModeEnabled(context))
        {
            return false;
        }

        if (!context.TryGet(GraphRuntimeContextKeys.currentGraph, out BaseGraph graph) || graph == null)
        {
            return false;
        }

        if (!graph.TryGetNodeById(node.nextNodeId, out BaseGraphNode nextNode) || nextNode is not ChoiceNode foundChoiceNode)
        {
            return false;
        }

        choiceNode = foundChoiceNode;
        return true;
    }

    private static bool IsImmediateChoiceModeEnabled(GraphExecutionContext context)
    {
        return context.TryGet(GraphRuntimeContextKeys.immediateChoiceAfterDialogue, out bool isEnabled) && isEnabled;
    }

    private static async UniTask<GraphNodeExecutionResult> ExecuteImmediateChoiceAsync(ChoiceNode choiceNode, GraphExecutionContext context, CancellationToken cancellationToken)
    {
        if (choiceNode.options == null)
        {
            return GraphNodeExecutionResult.Fault($"ChoiceNode '{choiceNode.Id}' options list is null.", GraphNodeExecutionErrorType.InvalidNode);
        }

        List<ChoiceOption> validOptions = new List<ChoiceOption>();
        for (int i = 0; i < choiceNode.options.Count; i++)
        {
            ChoiceOption option = choiceNode.options[i];
            if (option != null && !string.IsNullOrWhiteSpace(option.nextNodeId) && !string.IsNullOrWhiteSpace(option.label))
            {
                validOptions.Add(option);
            }
        }

        if (validOptions.Count == 0)
        {
            return GraphNodeExecutionResult.Fault($"ChoiceNode '{choiceNode.Id}' has no valid options.", GraphNodeExecutionErrorType.InvalidTransition);
        }

        if (context.ChoiceService == null)
        {
            Debug.LogWarning($"{BaseNodeExecutorConstants.LogPrefix} Choice service is not registered. Selecting first option as fallback for node '{choiceNode.Id}'.");
            return GraphNodeExecutionResult.ContinueTo(validOptions[0].nextNodeId);
        }

        List<GraphChoiceEntry> entries = new List<GraphChoiceEntry>(validOptions.Count);
        for (int i = 0; i < validOptions.Count; i++)
        {
            entries.Add(new GraphChoiceEntry(validOptions[i].label));
        }

        int selectedIndex = await context.ChoiceService.ShowAsync(entries, cancellationToken);
        if (selectedIndex < 0 || selectedIndex >= validOptions.Count)
        {
            return GraphNodeExecutionResult.Fault($"Choice service returned invalid index: {selectedIndex}.", GraphNodeExecutionErrorType.ServiceFailure);
        }

        return GraphNodeExecutionResult.ContinueTo(validOptions[selectedIndex].nextNodeId);
    }
}

public sealed class ChoiceNodeExecutor : GraphNodeExecutor<ChoiceNode>
{
    protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(ChoiceNode node, GraphExecutionContext context, CancellationToken cancellationToken)
    {
        if (node.options == null)
        {
            return UniTask.FromResult(GraphNodeExecutionResult.Fault($"ChoiceNode '{node.Id}' options list is null.", GraphNodeExecutionErrorType.InvalidNode));
        }

        List<ChoiceOption> validOptions = new List<ChoiceOption>();
        for (int i = 0; i < node.options.Count; i++)
        {
            ChoiceOption option = node.options[i];
            if (option != null && !string.IsNullOrWhiteSpace(option.nextNodeId) && !string.IsNullOrWhiteSpace(option.label))
            {
                validOptions.Add(option);
            }
        }

        if (validOptions.Count == 0)
        {
            return UniTask.FromResult(GraphNodeExecutionResult.Fault($"ChoiceNode '{node.Id}' has no valid options.", GraphNodeExecutionErrorType.InvalidTransition));
        }

        if (context.ChoiceService == null)
        {
            Debug.LogWarning($"{BaseNodeExecutorConstants.LogPrefix} Choice service is not registered. Selecting first option as fallback for node '{node.Id}'.");
            return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(validOptions[0].nextNodeId));
        }

        return ExecuteWithServiceAsync(validOptions, context.ChoiceService, cancellationToken);
    }

    private static async UniTask<GraphNodeExecutionResult> ExecuteWithServiceAsync(List<ChoiceOption> validOptions, IGraphChoiceService choiceService, CancellationToken cancellationToken)
    {
        List<GraphChoiceEntry> entries = new List<GraphChoiceEntry>(validOptions.Count);
        for (int i = 0; i < validOptions.Count; i++)
        {
            entries.Add(new GraphChoiceEntry(validOptions[i].label));
        }

        int selectedIndex = await choiceService.ShowAsync(entries, cancellationToken);
        if (selectedIndex < 0 || selectedIndex >= validOptions.Count)
        {
            return GraphNodeExecutionResult.Fault($"Choice service returned invalid index: {selectedIndex}.", GraphNodeExecutionErrorType.ServiceFailure);
        }

        return GraphNodeExecutionResult.ContinueTo(validOptions[selectedIndex].nextNodeId);
    }
}

public sealed class MapMarkerNodeExecutor : GraphNodeExecutor<MapMarkerNode>
{
    protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(MapMarkerNode node, GraphExecutionContext context, CancellationToken cancellationToken)
    {
        if (context.MapMarkerService != null)
        {
            context.MapMarkerService.ShowOrUpdateMarker(node.markerId, node.targetObjectName);
        }
        else
        {
            Debug.Log($"{BaseNodeExecutorConstants.LogPrefix} MapMarker fallback: markerId='{node.markerId}', target='{node.targetObjectName}'");
        }

        return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
    }
}

public sealed class PlayCutsceneNodeExecutor : GraphNodeExecutor<PlayCutsceneNode>
{
    protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(PlayCutsceneNode node, GraphExecutionContext context, CancellationToken cancellationToken)
    {
        if (context.CutsceneService == null)
        {
            Debug.Log($"{BaseNodeExecutorConstants.LogPrefix} PlayCutscene fallback: '{node.cutsceneReference}'");
            return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
        }

        return ExecuteWithServiceAsync(node, context.CutsceneService, cancellationToken);
    }

    private static async UniTask<GraphNodeExecutionResult> ExecuteWithServiceAsync(PlayCutsceneNode node, IGraphCutsceneService cutsceneService, CancellationToken cancellationToken)
    {
        await cutsceneService.PlayAsync(node.cutsceneReference, cancellationToken);
        return GraphNodeExecutionResult.ContinueTo(node.nextNodeId);
    }
}

public sealed class CheckpointNodeExecutor : GraphNodeExecutor<CheckpointNode>
{
    protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(CheckpointNode node, GraphExecutionContext context, CancellationToken cancellationToken)
    {
        if (context.CheckpointService == null)
        {
            Debug.Log($"{BaseNodeExecutorConstants.LogPrefix} Checkpoint fallback: action='{node.action}', checkpoint='{node.checkpointId}'");
            Debug.LogWarning($"{BaseNodeExecutorConstants.LogPrefix} Checkpoint service is not registered. Using fail branch fallback for node '{node.Id}'.");
            return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.failNodeId));
        }

        return ExecuteWithServiceAsync(node, context.CheckpointService, cancellationToken);
    }

    private static async UniTask<GraphNodeExecutionResult> ExecuteWithServiceAsync(CheckpointNode node, IGraphCheckpointService checkpointService, CancellationToken cancellationToken)
    {
        bool success = node.action == CheckpointAction.Clear
            ? await checkpointService.ClearAsync(node.checkpointId, cancellationToken)
            : await checkpointService.SaveAsync(node.checkpointId, cancellationToken);

        return GraphNodeExecutionResult.ContinueTo(success ? node.successNodeId : node.failNodeId);
    }
}

public sealed class StartQuestNodeExecutor : GraphNodeExecutor<StartQuestNode>
{
    protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(StartQuestNode node, GraphExecutionContext context, CancellationToken cancellationToken)
    {
        if (context.QuestService == null)
        {
            Debug.Log($"{BaseNodeExecutorConstants.LogPrefix} StartQuest fallback: quest='{node.questId}'");
            Debug.LogWarning($"{BaseNodeExecutorConstants.LogPrefix} Quest service is not registered. Using fail branch fallback for start quest node '{node.Id}'.");
            return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.failNodeId));
        }

        return ExecuteWithServiceAsync(node, context.QuestService, cancellationToken);
    }

    private static async UniTask<GraphNodeExecutionResult> ExecuteWithServiceAsync(StartQuestNode node, IGraphQuestService questService, CancellationToken cancellationToken)
    {
        bool success = await questService.StartQuestAsync(node.questId, cancellationToken);
        return GraphNodeExecutionResult.ContinueTo(success ? node.successNodeId : node.failNodeId);
    }
}

public sealed class CompleteQuestNodeExecutor : GraphNodeExecutor<CompleteQuestNode>
{
    protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(CompleteQuestNode node, GraphExecutionContext context, CancellationToken cancellationToken)
    {
        if (context.QuestService == null)
        {
            Debug.Log($"{BaseNodeExecutorConstants.LogPrefix} CompleteQuest fallback: quest='{node.questId}'");
            Debug.LogWarning($"{BaseNodeExecutorConstants.LogPrefix} Quest service is not registered. Using fail branch fallback for complete quest node '{node.Id}'.");
            return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.failNodeId));
        }

        return ExecuteWithServiceAsync(node, context.QuestService, cancellationToken);
    }

    private static async UniTask<GraphNodeExecutionResult> ExecuteWithServiceAsync(CompleteQuestNode node, IGraphQuestService questService, CancellationToken cancellationToken)
    {
        bool success = await questService.CompleteQuestAsync(node.questId, cancellationToken);
        return GraphNodeExecutionResult.ContinueTo(success ? node.successNodeId : node.failNodeId);
    }
}

public sealed class QuestStateConditionNodeExecutor : GraphNodeExecutor<QuestStateConditionNode>
{
    protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(QuestStateConditionNode node, GraphExecutionContext context, CancellationToken cancellationToken)
    {
        if (context.QuestService == null)
        {
            Debug.Log($"{BaseNodeExecutorConstants.LogPrefix} QuestStateCondition fallback: quest='{node.questId}', expected='{node.state}'");
            Debug.LogWarning($"{BaseNodeExecutorConstants.LogPrefix} Quest service is not registered. Using false branch fallback for condition node '{node.Id}'.");
            return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.falseNodeId));
        }

        return ExecuteWithServiceAsync(node, context.QuestService, cancellationToken);
    }

    private static async UniTask<GraphNodeExecutionResult> ExecuteWithServiceAsync(QuestStateConditionNode node, IGraphQuestService questService, CancellationToken cancellationToken)
    {
        QuestState actualState = await questService.GetQuestStateAsync(node.questId, cancellationToken);
        bool isMatch = actualState == node.state;
        return GraphNodeExecutionResult.ContinueTo(isMatch ? node.trueNodeId : node.falseNodeId);
    }
}
