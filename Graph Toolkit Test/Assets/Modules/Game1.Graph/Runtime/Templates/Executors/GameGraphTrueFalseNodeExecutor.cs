using System.Threading;
using Cysharp.Threading.Tasks;
using Game1.Graph.Runtime.Infrastructure;
using Graph.Core.Runtime;

namespace Game1.Graph.Runtime.Templates.Executors
{
	public abstract class GameGraphTrueFalseNodeExecutor<TNode> : GameGraphNodeExecutor<TNode>
		where TNode : GameGraphTrueFalseNode
	{
		protected sealed override async UniTask<GraphNodeExecutionResult> ExecuteAsync(
			TNode node,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			if (node == null)
			{
				return GraphNodeExecutionResult.Fault("Node is null.", GraphNodeExecutionErrorType.InvalidNode);
			}

			bool condition = await EvaluateConditionAsync(node, context, cancellationToken);
			return condition
				? GraphNodeExecutionResult.ContinueTo(node.trueNodeId)
				: GraphNodeExecutionResult.ContinueTo(node.falseNodeId);
		}

		protected abstract UniTask<bool> EvaluateConditionAsync(
			TNode node,
			GraphExecutionContext context,
			CancellationToken cancellationToken);

		protected static GraphNodeExecutionResult True(TNode node)
		{
			return GraphNodeExecutionResult.ContinueTo(node?.trueNodeId);
		}

		protected static GraphNodeExecutionResult False(TNode node)
		{
			return GraphNodeExecutionResult.ContinueTo(node?.falseNodeId);
		}
	}
}