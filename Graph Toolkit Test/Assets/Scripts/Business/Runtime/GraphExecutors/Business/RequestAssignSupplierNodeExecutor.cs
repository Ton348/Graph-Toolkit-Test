using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Game1.Graph.Runtime;

[GameGraphNodeExecutor]
public sealed class RequestAssignSupplierNodeExecutor : GameGraphServerRequestExecutor<RequestAssignSupplierNode>
{
	protected override UniTask<ServerActionResult> ExecuteRequestAsync(RequestAssignSupplierNode node, GameBootstrap bootstrap, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		return GameGraphExecutorContext.ExecuteServerAsync(context, bootstrap.GameServer.TryAssignSupplierAsync(node.lotId, node.supplierId));
	}
}

