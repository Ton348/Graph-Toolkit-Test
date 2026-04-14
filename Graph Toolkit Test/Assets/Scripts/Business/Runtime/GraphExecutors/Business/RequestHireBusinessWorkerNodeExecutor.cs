using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

[GameGraphNodeExecutor]
public sealed class RequestHireBusinessWorkerNodeExecutor : GameGraphServerRequestExecutor<RequestHireBusinessWorkerNode>
{
	protected override UniTask<ServerActionResult> ExecuteRequestAsync(RequestHireBusinessWorkerNode node, GameBootstrap bootstrap, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		return GameGraphExecutorContext.ExecuteServerAsync(context, bootstrap.GameServer.TryHireBusinessWorkerAsync(node.lotId, node.roleId, node.contactId));
	}
}

