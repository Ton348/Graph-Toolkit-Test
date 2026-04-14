using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

[GameGraphNodeExecutor]
public sealed class RequestTradeOfferNodeExecutor : GameGraphServerRequestExecutor<RequestTradeOfferNode>
{
	protected override UniTask<ServerActionResult> ExecuteRequestAsync(RequestTradeOfferNode node, GameBootstrap bootstrap, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		context?.Set(GraphContextKeys.BuildingLastRequestedId, node.buildingId);

		int offeredAmount = 0;
		if (bootstrap.GameDataRepository != null && !string.IsNullOrWhiteSpace(node.buildingId))
		{
			BuildingDefinitionData building = bootstrap.GameDataRepository.GetBuildingById(node.buildingId);
			if (building != null)
			{
				offeredAmount = Mathf.Max(0, building.purchaseCost);
			}
		}

		return GameGraphExecutorContext.ExecuteServerAsync(context, bootstrap.GameServer.TrySubmitTradeOfferAsync(node.buildingId, offeredAmount));
	}
}

