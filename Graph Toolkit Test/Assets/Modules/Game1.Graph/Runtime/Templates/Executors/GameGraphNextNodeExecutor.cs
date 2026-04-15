using Cysharp.Threading.Tasks;
using System.Threading;
using GraphCore.Runtime;
using Game1.Graph.Runtime;

using Game1.Graph.Runtime.Infrastructure;
using Game1.Graph.Runtime.Templates;
namespace Game1.Graph.Runtime.Templates.Executors
{
	public abstract class GameGraphNextNodeExecutor<TNode> : GameGraphNodeExecutor<TNode> where TNode : GameGraphNextNode
	{
		protected sealed override UniTask<GraphNodeExecutionResult> ExecuteAsync(TNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			if (node == null)
			{
				return UniTask.FromResult(GraphNodeExecutionResult.Fault("Node is null.", GraphNodeExecutionErrorType.InvalidNode));
			}

			return ExecuteNodeAsync(node, context, cancellationToken);
		}

		protected virtual UniTask<GraphNodeExecutionResult> ExecuteNodeAsync(TNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
		}

		protected static GraphNodeExecutionResult ContinueToNext(TNode node)
		{
			return GraphNodeExecutionResult.ContinueTo(node?.nextNodeId);
		}
	}
}
