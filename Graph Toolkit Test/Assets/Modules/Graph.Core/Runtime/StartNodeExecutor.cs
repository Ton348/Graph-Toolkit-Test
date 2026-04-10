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

public sealed class StartNodeExecutor : GraphNodeExecutor<StartNode>
{
	protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(StartNode node, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
	}
}
