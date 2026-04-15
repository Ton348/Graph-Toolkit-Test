using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Game1.Graph.Runtime;

[GameGraphNodeExecutor]
public sealed class RequestRentBusinessNodeExecutor : GameGraphServerRequestExecutor<RequestRentBusinessNode>
{
	protected override UniTask<ServerActionResult> ExecuteRequestAsync(RequestRentBusinessNode node, GameBootstrap bootstrap, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		return GameGraphExecutorContext.ExecuteServerAsync(context, bootstrap.GameServer.TryRentBusinessAsync(node.lotId));
	}
}

