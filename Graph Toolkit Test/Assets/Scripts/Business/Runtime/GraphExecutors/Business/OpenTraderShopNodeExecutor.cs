using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using GameGraph.Runtime.Business;
using Graph.Core.Runtime;
using Prototype.Business.Bootstrap;
using Prototype.Business.Data;
using Prototype.Business.Runtime.GraphExecutors.Infrastructure;
using Prototype.Business.Services;
using Sample.Runtime.UI;
using UnityEngine;

namespace Prototype.Business.Runtime.GraphExecutors.Business
{
	[GameGraphNodeExecutor]
	public sealed class OpenTraderShopNodeExecutor : GameGraphServerRequestExecutor<OpenTraderShopNode>
	{
		protected override async UniTask<ServerActionResult> ExecuteRequestAsync(
			OpenTraderShopNode node,
			GameBootstrap bootstrap,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			if (node == null || string.IsNullOrWhiteSpace(node.traderId))
			{
				return ServerActionResult.FailResult("trader_id_required", "traderId is required.");
			}

			TraderItemsResponse response = bootstrap?.BusinessActionFacade != null
				? await bootstrap.BusinessActionFacade.GetTraderItems(node.traderId)
				: await bootstrap.GameServer.TryGetTraderItemsAsync(node.traderId);
			if (response == null || !response.Success)
			{
				return ServerActionResult.FailResult(response?.ErrorCode ?? "trader_items_failed",
					response?.Message ?? "Trader items request failed.");
			}

			TraderShopUIService ui = UnityEngine.Object.FindAnyObjectByType<TraderShopUIService>(FindObjectsInactive.Include);
			if (ui == null)
			{
				return ServerActionResult.FailResult("trader_shop_ui_missing", "Trader shop UI not found.");
			}

			List<TraderShopItemData> items = new();
			for (int i = 0; i < response.Items.Count; i++)
			{
				TraderItemDefinitionData item = response.Items[i];
				if (item == null)
				{
					continue;
				}

				items.Add(new TraderShopItemData
				{
					itemId = item.id,
					category = item.category,
					name = item.name,
					description = item.description,
					price = item.price,
					storageCapacity = item.storageCapacity,
					cashCapacity = item.cashCapacity,
					shelfCapacity = item.shelfCapacity,
					isOwned = bootstrap?.PlayerStateSync != null && bootstrap.PlayerStateSync.HasItem(item.id)
				});
			}

			var tcs = new UniTaskCompletionSource<string>();
			ui.ShowTrader(
				response.TraderName,
				bootstrap?.PlayerStateSync != null ? bootstrap.PlayerStateSync.Money : 0,
				items,
				itemId => tcs.TrySetResult(itemId),
				() => tcs.TrySetResult(string.Empty));

			string selectedItemId = await tcs.Task.AttachExternalCancellation(cancellationToken);
			if (string.IsNullOrWhiteSpace(selectedItemId))
			{
				return ServerActionResult.FailResult("purchase_cancelled", "Purchase cancelled.");
			}

			if (bootstrap.BusinessActionFacade != null)
			{
				return await bootstrap.BusinessActionFacade.BuyItem(node.traderId, selectedItemId).AsUniTask();
			}

			return await bootstrap.GameServer.TryBuyItemAsync(node.traderId, selectedItemId).AsUniTask();
		}
	}
}
