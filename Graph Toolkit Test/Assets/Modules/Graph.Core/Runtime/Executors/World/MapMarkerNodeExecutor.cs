using System.Threading;
using Cysharp.Threading.Tasks;
using Graph.Core.Runtime.Executors.Templates;
using Graph.Core.Runtime.Nodes.World;

namespace Graph.Core.Runtime.Executors.World
{
	public sealed class MapMarkerNodeExecutor : CoreGraphNextNodeExecutor<MapMarkerNode>
	{
		protected override UniTask<GraphNodeExecutionResult> ExecuteNodeAsync(
			MapMarkerNode node,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			if (context.MapMarkerService != null)
			{
				context.MapMarkerService.ShowOrUpdateMarker(node.markerId, node.targetObjectName);
			}

			return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
		}
	}
}