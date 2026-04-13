using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public abstract class GameGraphNodeExecutor<TNode> : IGraphNodeExecutor where TNode : GameGraphNode
{
	public Type NodeType => typeof(TNode);

	public UniTask<GraphNodeExecutionResult> ExecuteAsync(BaseGraphNode node, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		if (node is not TNode typedNode)
		{
			return UniTask.FromResult(GraphNodeExecutionResult.Fault(
				$"Expected node type '{typeof(TNode).Name}', got '{node?.GetType().Name ?? "null"}'.",
				GraphNodeExecutionErrorType.InvalidNode));
		}

		return ExecuteAsync(typedNode, context, cancellationToken);
	}

	protected abstract UniTask<GraphNodeExecutionResult> ExecuteAsync(TNode node, GraphExecutionContext context, CancellationToken cancellationToken);
}
