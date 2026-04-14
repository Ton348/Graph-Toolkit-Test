using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

[GameGraphNodeExecutor]
public sealed class RequestCloseBusinessNodeExecutor : GameGraphServerRequestExecutor<RequestCloseBusinessNode>
{
	protected override UniTask<ServerActionResult> ExecuteRequestAsync(RequestCloseBusinessNode node, GameBootstrap bootstrap, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		return GameGraphExecutorContext.ExecuteServerAsync(context, bootstrap.GameServer.TryCloseBusinessAsync(node.lotId));
	}
}

