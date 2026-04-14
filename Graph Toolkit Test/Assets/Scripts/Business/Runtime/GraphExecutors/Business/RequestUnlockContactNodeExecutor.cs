using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

[GameGraphNodeExecutor]
public sealed class RequestUnlockContactNodeExecutor : GameGraphServerRequestExecutor<RequestUnlockContactNode>
{
	protected override UniTask<ServerActionResult> ExecuteRequestAsync(RequestUnlockContactNode node, GameBootstrap bootstrap, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		return GameGraphExecutorContext.ExecuteServerAsync(context, bootstrap.GameServer.TryUnlockContactAsync(node.contactId));
	}
}
