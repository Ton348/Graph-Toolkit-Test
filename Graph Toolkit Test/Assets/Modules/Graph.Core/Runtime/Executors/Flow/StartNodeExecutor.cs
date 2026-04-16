using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Nodes.Flow;
using System.Threading;

namespace GraphCore.Runtime.Executors.Flow
{
	public sealed class StartNodeExecutor : BaseGraphNodeExecutor<StartNode>
	{
		protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(StartNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
		}
	}
}
