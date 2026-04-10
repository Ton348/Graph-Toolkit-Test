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

public sealed class FinishNodeExecutor : GraphNodeExecutor<FinishNode>
{
	protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(FinishNode node, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		context?.DialogueService?.EndConversation();
		return UniTask.FromResult(GraphNodeExecutionResult.Stop("Finish node reached."));
	}
}
