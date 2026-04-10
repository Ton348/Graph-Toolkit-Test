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
