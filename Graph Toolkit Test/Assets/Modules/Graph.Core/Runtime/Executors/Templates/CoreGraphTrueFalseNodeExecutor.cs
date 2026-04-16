using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Templates;

namespace GraphCore.Runtime.Executors.Templates
{
	public abstract class CoreGraphTrueFalseNodeExecutor<TNode> : BaseGraphNodeExecutor<TNode>
		where TNode : CoreGraphTrueFalseNode
	{
		protected sealed override async UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(
			TNode node,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			bool condition = await EvaluateConditionAsync(node, context, cancellationToken);
			return GraphNodeExecutionResult.ContinueTo(condition ? node.trueNodeId : node.falseNodeId);
		}

		protected abstract UniTask<bool> EvaluateConditionAsync(
			TNode node,
			GraphExecutionContext context,
			CancellationToken cancellationToken);
	}
}