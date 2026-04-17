using System.Threading;
using Cysharp.Threading.Tasks;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using GameGraph.Runtime.Business;
using Graph.Core.Runtime;
using Prototype.Business.Bootstrap;
using Prototype.Business.Runtime.GraphExecutors.Infrastructure;
using Prototype.Business.Services;

namespace Prototype.Business.Runtime.GraphExecutors.Business
{
	[GameGraphNodeExecutor]
	public sealed class RequestBuyBuildingNodeExecutor : GameGraphServerRequestExecutor<RequestBuyBuildingNode>
	{
		protected override UniTask<ServerActionResult> ExecuteRequestAsync(
			RequestBuyBuildingNode node,
			GameBootstrap bootstrap,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			context?.Set(GraphContextKeys.buildingLastRequestedId, node.buildingId);
			if (!string.IsNullOrWhiteSpace(node.questId))
			{
				context?.Set(GraphContextKeys.questLastRequestedId, node.questId);
			}

			return GameGraphExecutorContext.ExecuteServerAsync(context,
				bootstrap.GameServer.TryBuyBuildingAsync(node.buildingId, node.questAction, node.questId));
		}
	}
}