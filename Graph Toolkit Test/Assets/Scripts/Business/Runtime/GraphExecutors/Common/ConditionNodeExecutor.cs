using System.Threading;
using Cysharp.Threading.Tasks;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using Game1.Graph.Runtime.Templates.Executors;
using Graph.Core.Runtime;

[GameGraphNodeExecutorAttribute]
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