using Cysharp.Threading.Tasks;
using GraphCore.Runtime;
using System.Threading;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using Game1.Graph.Runtime.Templates.Executors;

namespace Game1.Graph.Templates.Samples
{
	[GameGraphNodeExecutorAttribute]
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
