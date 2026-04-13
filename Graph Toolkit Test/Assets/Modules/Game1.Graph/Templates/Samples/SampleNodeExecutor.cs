using System.Threading;
using Cysharp.Threading.Tasks;

[GameGraphNodeExecutor]
public sealed class SampleNodeExecutor : GameGraphNextNodeExecutor<SampleNode>
{
	protected override UniTask<GraphNodeExecutionResult> ExecuteNodeAsync(SampleNode node, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		if (!node.enabled)
		{
			return UniTask.FromResult(GraphNodeExecutionResult.Stop("Sample node is disabled."));
		}

		return UniTask.FromResult(ContinueToNext(node));
	}
}
