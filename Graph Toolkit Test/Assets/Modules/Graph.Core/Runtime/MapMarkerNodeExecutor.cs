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

public sealed class MapMarkerNodeExecutor : GraphNodeExecutor<MapMarkerNode>
{
	protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(MapMarkerNode node, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		if (context.MapMarkerService != null)
		{
			context.MapMarkerService.ShowOrUpdateMarker(node.markerId, node.targetObjectName);
		}
		else
		{
			Debug.Log($"{BaseNodeExecutorConstants.LogPrefix} MapMarker fallback: markerId='{node.markerId}', target='{node.targetObjectName}'");
		}

		return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
	}
}
