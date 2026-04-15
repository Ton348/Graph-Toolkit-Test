using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.Runtime;
using UnityEngine;
using Game1.Graph.Runtime;

using Game1.Graph.Runtime.Infrastructure;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
[GameGraphNodeExecutorAttribute]
public sealed class RequestTradeOfferNodeExecutor : GameGraphServerRequestExecutor<RequestTradeOfferNode>
{
	protected override async UniTask<ServerActionResult> ExecuteRequestAsync(RequestTradeOfferNode node, GameBootstrap bootstrap, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		context?.Set(GraphContextKeys.BuildingLastRequestedId, node.buildingId);

		int fallbackOffer = 0;
		string buildingLabel = node.buildingId;
		if (bootstrap.GameDataRepository != null && !string.IsNullOrWhiteSpace(node.buildingId))
		{
			BuildingDefinitionData building = bootstrap.GameDataRepository.GetBuildingById(node.buildingId);
			if (building != null)
			{
				fallbackOffer = Mathf.Max(1, building.purchaseCost);
				buildingLabel = string.IsNullOrWhiteSpace(building.displayName) ? node.buildingId : building.displayName;
			}
		}

		int offeredAmount = await ResolveOfferAmountAsync(buildingLabel, fallbackOffer, cancellationToken);
		return await GameGraphExecutorContext.ExecuteServerAsync(context, bootstrap.GameServer.TrySubmitTradeOfferAsync(node.buildingId, offeredAmount));
	}

	private static async UniTask<int> ResolveOfferAmountAsync(string buildingLabel, int fallbackOffer, CancellationToken cancellationToken)
	{
		TradeOfferUIService ui = Object.FindAnyObjectByType<TradeOfferUIService>(FindObjectsInactive.Include);
		if (ui == null)
		{
			return Mathf.Max(1, fallbackOffer);
		}

		var tcs = new UniTaskCompletionSource<int>();
		bool completed = false;
		ui.ShowOffer(buildingLabel, Mathf.Max(1, fallbackOffer), amount =>
		{
			if (completed)
			{
				return;
			}

			completed = true;
			tcs.TrySetResult(Mathf.Max(1, amount));
		});

		using (cancellationToken.Register(() =>
		{
			if (completed)
			{
				return;
			}

			completed = true;
			tcs.TrySetCanceled();
		}))
		{
			try
			{
				return await tcs.Task;
			}
			catch
			{
				ui.Hide();
				return Mathf.Max(1, fallbackOffer);
			}
		}
	}
}
