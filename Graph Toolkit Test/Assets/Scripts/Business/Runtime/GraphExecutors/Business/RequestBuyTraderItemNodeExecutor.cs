using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using GameGraph.Runtime.Business;
using Graph.Core.Runtime;
using Prototype.Business.Bootstrap;
using Prototype.Business.Data;
using Prototype.Business.Runtime.GraphExecutors.Infrastructure;
using Prototype.Business.Services;

namespace Prototype.Business.Runtime.GraphExecutors.Business
{
	[GameGraphNodeExecutor]
	public sealed class RequestBuyTraderItemNodeExecutor : GameGraphServerRequestExecutor<RequestBuyTraderItemNode>
	{
		protected override async UniTask<ServerActionResult> ExecuteRequestAsync(
			RequestBuyTraderItemNode node,
			GameBootstrap bootstrap,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			if (node == null || string.IsNullOrWhiteSpace(node.itemId))
			{
				return ServerActionResult.FailResult("item_id_required", "itemId is required.");
			}

			string traderId = ResolveTraderIdByItem(bootstrap, node.itemId);
			if (string.IsNullOrWhiteSpace(traderId))
			{
				return ServerActionResult.FailResult("trader_not_found", "Trader for item was not found.");
			}

			if (bootstrap.BusinessActionFacade != null)
			{
				return await bootstrap.BusinessActionFacade.BuyItem(traderId, node.itemId).AsUniTask();
			}

			return await bootstrap.GameServer.TryBuyItemAsync(traderId, node.itemId).AsUniTask();
		}

		private static string ResolveTraderIdByItem(GameBootstrap bootstrap, string itemId)
		{
			if (bootstrap?.BusinessDefinitionsRepository == null || string.IsNullOrWhiteSpace(itemId))
			{
				return null;
			}

			foreach (TraderDefinitionData trader in bootstrap.BusinessDefinitionsRepository.GetAllTraders())
			{
				if (trader?.items == null || string.IsNullOrWhiteSpace(trader.id))
				{
					continue;
				}

				for (int i = 0; i < trader.items.Count; i++)
				{
					TraderItemDefinitionData item = trader.items[i];
					if (item != null && string.Equals(item.id, itemId, StringComparison.Ordinal))
					{
						return trader.id;
					}
				}
			}

			return null;
		}
	}
}
