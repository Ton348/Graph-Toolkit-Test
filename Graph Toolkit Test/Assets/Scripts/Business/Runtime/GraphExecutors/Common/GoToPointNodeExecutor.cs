using System.Threading;
using Cysharp.Threading.Tasks;

[GameGraphNodeExecutor]
public sealed class GoToPointNodeExecutor : GameGraphNextNodeExecutor<GoToPointNode>
{
	protected override UniTask<GraphNodeExecutionResult> ExecuteNodeAsync(GoToPointNode node, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		if (context != null && context.TryGet(GraphContextKeys.RuntimeMapMarkerService, out MapMarkerService markerService) && markerService != null)
		{
			markerService.ShowMarker(node.markerId, node.targetTransform, node.markerId);
		}

		return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
	}
}
