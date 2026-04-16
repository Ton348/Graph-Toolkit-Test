using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Nodes.Flow;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GraphCore.Runtime.Executors.Flow
{
	public sealed class RandomNodeExecutor : BaseGraphNodeExecutor<RandomNode>
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
}
