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
	public sealed class FinishNodeExecutor : BaseGraphNodeExecutor<FinishNode>
	{
		protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(FinishNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			context?.DialogueService?.EndConversation();
			return UniTask.FromResult(GraphNodeExecutionResult.Stop("Finish node reached."));
		}
	}
}
