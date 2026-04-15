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
	public sealed class StartNodeExecutor : BaseGraphNodeExecutor<StartNode>
	{
		protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(StartNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
		}
	}
}
