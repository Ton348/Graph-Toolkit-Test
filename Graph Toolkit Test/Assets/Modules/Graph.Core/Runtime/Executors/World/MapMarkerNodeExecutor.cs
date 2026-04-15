using Cysharp.Threading.Tasks;
using GraphCore.BaseNodes.Runtime.World;
using System.Threading;

public sealed class MapMarkerNodeExecutor : BaseGraphNodeExecutor<MapMarkerNode>
{
	protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(MapMarkerNode node, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		if (context.MapMarkerService != null)
		{
			context.MapMarkerService.ShowOrUpdateMarker(node.markerId, node.targetObjectName);
		}

		return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
	}
}
