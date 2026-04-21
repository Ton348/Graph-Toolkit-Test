using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using Game1.Graph.Runtime.Templates;
using GameGraph.Runtime.Business;
using Graph.Core.Runtime;
using Prototype.Business.Bootstrap;
using Prototype.Business.Data;
using Prototype.Business.Services;
using Prototype.Business.Runtime.GraphExecutors.Infrastructure;

namespace GameGraph.Runtime.Business
{
	[Serializable]
	public sealed class RequestBuyTraderItemNode : GameGraphSuccessFailNode
	{
		public string itemId;
	}
}

namespace Prototype.Business.Runtime.GraphExecutors.Business
{
	[GameGraphNodeExecutor]
	public sealed class RequestBuyTraderItemNodeExecutor : GameGraphServerRequestExecutor<RequestBuyTraderItemNode>
	{
		protected override UniTask<ServerActionResult> ExecuteRequestAsync(
			RequestBuyTraderItemNode node,
			GameBootstrap bootstrap,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			if (node == null || string.IsNullOrWhiteSpace(node.itemId))
			{
				return UniTask.FromResult(ServerActionResult.FailResult("item_id_required", "itemId is required."));
			}

			string traderId = ResolveTraderIdByItem(bootstrap, node.itemId);
			if (string.IsNullOrWhiteSpace(traderId))
			{
				return UniTask.FromResult(ServerActionResult.FailResult("trader_not_found", "Trader for item was not found."));
			}

			if (bootstrap.BusinessActionFacade != null)
			{
				return bootstrap.BusinessActionFacade.BuyItem(traderId, node.itemId).AsUniTask();
			}

			return bootstrap.GameServer.TryBuyItemAsync(traderId, node.itemId).AsUniTask();
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

#if UNITY_EDITOR
namespace GameGraph.Editor.Business
{
	using Graph.Core.Editor;
	using Unity.GraphToolkit.Editor;

	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class RequestBuyTraderItemNodeModel : GameGraphSuccessFailNodeModel
	{
		public const string ItemIdOption = "ItemId";

		protected override string defaultTitle => "Купить предмет у торговца";
		protected override string defaultDescription => "Запрашивает покупку предмета у торговца.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(ItemIdOption).WithDisplayName("ItemId");
		}
	}
}

namespace GameGraph.Editor.Converters.Business
{
	using Game1.Graph.Editor.Infrastructure.Converters;

	[GameGraphNodeConverter]
	public sealed class RequestBuyTraderItemNodeConverter :
		GameGraphNodeConverterBase<GameGraph.Editor.Business.RequestBuyTraderItemNodeModel, RequestBuyTraderItemNode>
	{
		protected override bool TryConvert(
			GameGraph.Editor.Business.RequestBuyTraderItemNodeModel editorNodeModel,
			out RequestBuyTraderItemNode runtimeNode)
		{
			runtimeNode = new RequestBuyTraderItemNode
			{
				itemId = GetOptionValue<string>(editorNodeModel,
					GameGraph.Editor.Business.RequestBuyTraderItemNodeModel.ItemIdOption)
			};
			return true;
		}
	}
}
#endif
