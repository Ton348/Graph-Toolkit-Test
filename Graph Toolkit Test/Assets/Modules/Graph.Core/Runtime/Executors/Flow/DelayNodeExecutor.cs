using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Nodes.Cinematics;
using GraphCore.Runtime.Nodes.Flow;
using GraphCore.Runtime.Nodes.Server;
using GraphCore.Runtime.Nodes.UI;
using GraphCore.Runtime.Nodes.Utility;
using GraphCore.Runtime.Nodes.World;
using System.Collections.Generic;
using System.Threading;
using System;
using UnityEngine;
using GraphCore.Runtime;

namespace GraphCore.Runtime.Executors.Flow
{
	public sealed class DelayNodeExecutor : BaseGraphNodeExecutor<DelayNode>
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
}
