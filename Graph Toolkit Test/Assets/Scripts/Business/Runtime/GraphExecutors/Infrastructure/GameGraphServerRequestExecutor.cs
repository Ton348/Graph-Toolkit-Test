using System.Threading;
using Cysharp.Threading.Tasks;
using Game1.Graph.Runtime;

public abstract class GameGraphServerRequestExecutor<TNode> : GameGraphSuccessFailNodeExecutor<TNode>
	where TNode : GameGraphSuccessFailNode
{
	protected sealed override async UniTask<bool> EvaluateSuccessAsync(TNode node, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		if (!GameGraphExecutorContext.TryGetBootstrap(context, out GameBootstrap bootstrap) || bootstrap.GameServer == null)
		{
			return false;
		}

		ServerActionResult result = await ExecuteRequestAsync(node, bootstrap, context, cancellationToken);
		return result != null && result.Success;
	}

	protected abstract UniTask<ServerActionResult> ExecuteRequestAsync(TNode node, GameBootstrap bootstrap, GraphExecutionContext context, CancellationToken cancellationToken);
}
