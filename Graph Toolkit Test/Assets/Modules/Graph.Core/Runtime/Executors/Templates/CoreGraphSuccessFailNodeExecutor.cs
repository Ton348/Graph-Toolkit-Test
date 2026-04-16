using System.Threading;
using Cysharp.Threading.Tasks;
using Graph.Core.Runtime.Templates;

namespace Graph.Core.Runtime.Executors.Templates
{
	public abstract class CoreGraphSuccessFailNodeExecutor<TNode> : BaseGraphNodeExecutor<TNode>
		where TNode : CoreGraphSuccessFailNode
	{
		protected sealed override async UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(
			TNode node,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			bool success = await EvaluateSuccessAsync(node, context, cancellationToken);
			return GraphNodeExecutionResult.ContinueTo(success ? node.successNodeId : node.failNodeId);
		}

		protected abstract UniTask<bool> EvaluateSuccessAsync(
			TNode node,
			GraphExecutionContext context,
			CancellationToken cancellationToken);
	}
}