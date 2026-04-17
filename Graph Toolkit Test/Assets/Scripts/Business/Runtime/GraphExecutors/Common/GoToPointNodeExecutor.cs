using System.Threading;
using Cysharp.Threading.Tasks;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using Game1.Graph.Runtime.Templates.Executors;
using GameGraph.Runtime.Common;
using Graph.Core.Runtime;
using Sample.Runtime.Services;

namespace Prototype.Business.Runtime.GraphExecutors.Common
{
	[GameGraphNodeExecutor]
	public sealed class GoToPointNodeExecutor : GameGraphNextNodeExecutor<GoToPointNode>
	{
		protected override UniTask<GraphNodeExecutionResult> ExecuteNodeAsync(
			GoToPointNode node,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			if (context != null &&
			    context.TryGet(GraphContextKeys.runtimeMapMarkerService, out MapMarkerService markerService) &&
			    markerService != null)
			{
				markerService.ShowMarker(node.markerId, node.targetTransform, node.markerId);
			}

			return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
		}
	}
}