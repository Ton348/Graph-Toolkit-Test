using System.Threading;
using Cysharp.Threading.Tasks;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using Game1.Graph.Runtime.Templates.Executors;
using GameGraph.Runtime.Common;
using Graph.Core.Runtime;
using Prototype.Business.Bootstrap;
using Prototype.Business.Runtime.GraphExecutors.Infrastructure;
using Prototype.Business.Services;

namespace Prototype.Business.Runtime.GraphExecutors.Common
{
	[GameGraphNodeExecutor]
	public sealed class ConditionNodeExecutor : GameGraphTrueFalseNodeExecutor<ConditionNode>
	{
		protected override UniTask<bool> EvaluateConditionAsync(
			ConditionNode node,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			PlayerStateSync playerStateSync = null;
			if (GameGraphExecutorContext.TryGetBootstrap(context, out GameBootstrap bootstrap))
			{
				playerStateSync = bootstrap.PlayerStateSync;
			}

			bool result = ConditionEvaluator.EvaluateCondition(node, playerStateSync);
			context?.Set(GraphContextKeys.conditionLastResult, result);
			return UniTask.FromResult(result);
		}
	}
}