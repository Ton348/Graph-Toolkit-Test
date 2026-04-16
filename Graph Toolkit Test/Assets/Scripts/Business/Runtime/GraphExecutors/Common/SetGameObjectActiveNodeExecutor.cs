using System.Threading;
using Cysharp.Threading.Tasks;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using Game1.Graph.Runtime.Templates.Executors;
using GameGraph.Runtime.Common;
using Graph.Core.Runtime;
using Prototype.Business.Bootstrap;
using Prototype.Business.Runtime.GraphExecutors.Infrastructure;

namespace Prototype.Business.Runtime.GraphExecutors.Common
{
	[GameGraphNodeExecutor]
	public sealed class SetGameObjectActiveNodeExecutor : GameGraphNextNodeExecutor<SetGameObjectActiveNode>
	{
		protected override async UniTask<GraphNodeExecutionResult> ExecuteNodeAsync(
			SetGameObjectActiveNode node,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			if (!GameGraphExecutorContext.TryGetBootstrap(context, out GameBootstrap bootstrap) ||
			    bootstrap.GameServer == null)
			{
				return GraphNodeExecutionResult.ContinueTo(node.nextNodeId);
			}

			string siteId = string.IsNullOrWhiteSpace(node.siteId) ? null : node.siteId.Trim();
			if (string.IsNullOrEmpty(siteId))
			{
				return GraphNodeExecutionResult.ContinueTo(node.nextNodeId);
			}

			if (node.isActive)
			{
				string visualId = !string.IsNullOrWhiteSpace(node.visualId)
					? node.visualId.Trim()
					: node.targetObject != null
						? node.targetObject.name
						: null;

				if (string.IsNullOrWhiteSpace(visualId))
				{
					return GraphNodeExecutionResult.ContinueTo(node.nextNodeId);
				}

				await GameGraphExecutorContext.ExecuteServerAsync(context,
					bootstrap.GameServer.TryConstructSiteVisualAsync(siteId, visualId));
			}
			else
			{
				await GameGraphExecutorContext.ExecuteServerAsync(context,
					bootstrap.GameServer.TryRemoveSiteVisualAsync(siteId));
			}

			return GraphNodeExecutionResult.ContinueTo(node.nextNodeId);
		}
	}
}