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
