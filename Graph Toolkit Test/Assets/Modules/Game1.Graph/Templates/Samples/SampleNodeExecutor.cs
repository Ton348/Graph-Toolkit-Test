using Cysharp.Threading.Tasks;
using System.Threading;

namespace Game1.Graph.Runtime
{
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
}
