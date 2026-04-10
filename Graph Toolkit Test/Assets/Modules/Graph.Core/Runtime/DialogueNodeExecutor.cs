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

		if (!context.TryGet(GraphRuntimeContextKeys.currentGraph, out CommonGraph graph) || graph == null)
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
