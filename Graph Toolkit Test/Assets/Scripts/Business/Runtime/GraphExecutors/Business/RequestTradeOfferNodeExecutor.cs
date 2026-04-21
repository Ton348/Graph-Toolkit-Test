using System.Threading;
using Cysharp.Threading.Tasks;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using GameGraph.Runtime.Business;
using Graph.Core.Runtime;
using Prototype.Business.Bootstrap;
using Prototype.Business.Runtime.GraphExecutors.Infrastructure;
using Prototype.Business.Services;
using Sample.Runtime.GameData;
using Sample.Runtime.UI;
using UnityEngine;

namespace Prototype.Business.Runtime.GraphExecutors.Business
{
	[GameGraphNodeExecutor]
	public sealed class RequestTradeOfferNodeExecutor : GameGraphServerRequestExecutor<RequestTradeOfferNode>
	{
		protected override async UniTask<ServerActionResult> ExecuteRequestAsync(
			RequestTradeOfferNode node,
			GameBootstrap bootstrap,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			context?.Set(GraphContextKeys.buildingLastRequestedId, node.buildingId);

			var fallbackOffer = 0;
			string buildingLabel = node.buildingId;
			if (bootstrap.GameDataRepository != null && !string.IsNullOrWhiteSpace(node.buildingId))
			{
				BuildingDefinitionData building = bootstrap.GameDataRepository.GetBuildingById(node.buildingId);
				if (building != null)
				{
					fallbackOffer = Mathf.Max(1, building.purchaseCost);
					buildingLabel = string.IsNullOrWhiteSpace(building.displayName)
						? node.buildingId
						: building.displayName;
				}
			}

			int offeredAmount = await ResolveOfferAmountAsync(buildingLabel, fallbackOffer, cancellationToken);
			return await GameGraphExecutorContext.ExecuteServerAsync(context,
				bootstrap.GameServer.TrySubmitTradeOfferAsync(node.buildingId, offeredAmount));
		}

		private static async UniTask<int> ResolveOfferAmountAsync(
			string buildingLabel,
			int fallbackOffer,
			CancellationToken cancellationToken)
		{
			var ui = Object.FindAnyObjectByType<TradeOfferUiservice>(FindObjectsInactive.Include);
			if (ui == null)
			{
				return await ResolveOfferAmountViaLegacyUiAsync(buildingLabel, fallbackOffer, cancellationToken);
			}

			var tcs = new UniTaskCompletionSource<int>();
			var completed = false;
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

		private static async UniTask<int> ResolveOfferAmountViaLegacyUiAsync(
			string buildingLabel,
			int fallbackOffer,
			CancellationToken cancellationToken)
		{
			MonoBehaviour[] allBehaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,
				FindObjectsSortMode.None);
			MonoBehaviour legacyUi = null;
			for (int i = 0; i < allBehaviours.Length; i++)
			{
				MonoBehaviour behaviour = allBehaviours[i];
				if (behaviour != null && behaviour.GetType().Name == "TradeOfferUIService")
				{
					legacyUi = behaviour;
					break;
				}
			}

			if (legacyUi == null)
			{
				return Mathf.Max(1, fallbackOffer);
			}

			System.Reflection.MethodInfo showOffer = legacyUi.GetType()
				.GetMethod("ShowOffer", new[] { typeof(string), typeof(int), typeof(System.Action<int>) });
			System.Reflection.MethodInfo hide = legacyUi.GetType().GetMethod("Hide", System.Type.EmptyTypes);
			if (showOffer == null)
			{
				return Mathf.Max(1, fallbackOffer);
			}

			var tcs = new UniTaskCompletionSource<int>();
			var completed = false;
			var minOffer = Mathf.Max(1, fallbackOffer);
			System.Action<int> callback = amount =>
			{
				if (completed)
				{
					return;
				}

				completed = true;
				tcs.TrySetResult(Mathf.Max(1, amount));
			};

			try
			{
				showOffer.Invoke(legacyUi, new object[] { buildingLabel, minOffer, callback });
			}
			catch
			{
				return minOffer;
			}

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
					hide?.Invoke(legacyUi, null);
					return minOffer;
				}
			}
		}
	}
}
