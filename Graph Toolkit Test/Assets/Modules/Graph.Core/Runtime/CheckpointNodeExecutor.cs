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
