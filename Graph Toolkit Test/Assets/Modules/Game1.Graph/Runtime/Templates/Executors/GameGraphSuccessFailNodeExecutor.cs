using System.Threading;
using Cysharp.Threading.Tasks;

public abstract class GameGraphSuccessFailNodeExecutor<TNode> : GameGraphNodeExecutor<TNode> where TNode : GameGraphSuccessFailNode
{
	protected sealed override async UniTask<GraphNodeExecutionResult> ExecuteAsync(TNode node, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		if (node == null)
		{
			return GraphNodeExecutionResult.Fault("Node is null.", GraphNodeExecutionErrorType.InvalidNode);
		}

		bool isSuccess = await EvaluateSuccessAsync(node, context, cancellationToken);
		return isSuccess
			? GraphNodeExecutionResult.ContinueTo(node.successNodeId)
			: GraphNodeExecutionResult.ContinueTo(node.failNodeId);
	}

	protected abstract UniTask<bool> EvaluateSuccessAsync(TNode node, GraphExecutionContext context, CancellationToken cancellationToken);

	protected static GraphNodeExecutionResult Success(TNode node)
	{
		return GraphNodeExecutionResult.ContinueTo(node?.successNodeId);
	}

	protected static GraphNodeExecutionResult Fail(TNode node)
	{
		return GraphNodeExecutionResult.ContinueTo(node?.failNodeId);
	}
}
