using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.Runtime;
using UnityEngine;
using Game1.Graph.Runtime;

using Game1.Graph.Runtime.Infrastructure;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
[GameGraphNodeExecutorAttribute]
public sealed class RequestBuyBuildingNodeExecutor : GameGraphServerRequestExecutor<RequestBuyBuildingNode>
{
	protected override UniTask<ServerActionResult> ExecuteRequestAsync(RequestBuyBuildingNode node, GameBootstrap bootstrap, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		context?.Set(GraphContextKeys.BuildingLastRequestedId, node.buildingId);
		if (!string.IsNullOrWhiteSpace(node.questId))
		{
			context?.Set(GraphContextKeys.QuestLastRequestedId, node.questId);
		}

		return GameGraphExecutorContext.ExecuteServerAsync(context, bootstrap.GameServer.TryBuyBuildingAsync(node.buildingId, node.questAction, node.questId));
	}
}

