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
