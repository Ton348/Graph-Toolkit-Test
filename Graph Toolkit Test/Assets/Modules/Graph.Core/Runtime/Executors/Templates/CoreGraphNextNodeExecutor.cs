using System.Threading;
using Cysharp.Threading.Tasks;
using Graph.Core.Runtime.Templates;

namespace Graph.Core.Runtime.Executors.Templates
{
	public abstract class CoreGraphNextNodeExecutor<TNode> : BaseGraphNodeExecutor<TNode>
		where TNode : CoreGraphNextNode
	{
		protected sealed override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(
			TNode node,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			return ExecuteNodeAsync(node, context, cancellationToken);
		}

		protected virtual UniTask<GraphNodeExecutionResult> ExecuteNodeAsync(
			TNode node,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
		}

		protected static GraphNodeExecutionResult ContinueToNext(TNode node)
		{
			return GraphNodeExecutionResult.ContinueTo(node?.nextNodeId);
		}
	}
}