using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Executors.Templates;
using GraphCore.Runtime.Nodes.World;
using System.Threading;

namespace GraphCore.Runtime.Executors.World
{
	public sealed class MapMarkerNodeExecutor : CoreGraphNextNodeExecutor<MapMarkerNode>
	{
		protected override UniTask<GraphNodeExecutionResult> ExecuteNodeAsync(MapMarkerNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			if (context.MapMarkerService != null)
			{
				context.MapMarkerService.ShowOrUpdateMarker(node.markerId, node.targetObjectName);
			}

			return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
		}
	}
}
