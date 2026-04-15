using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.Runtime;
using UnityEngine;
using Game1.Graph.Runtime;

using Game1.Graph.Runtime.Infrastructure;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
[GameGraphNodeExecutorAttribute]
public sealed class RequestAssignBusinessTypeNodeExecutor : GameGraphServerRequestExecutor<RequestAssignBusinessTypeNode>
{
	protected override UniTask<ServerActionResult> ExecuteRequestAsync(RequestAssignBusinessTypeNode node, GameBootstrap bootstrap, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		return GameGraphExecutorContext.ExecuteServerAsync(context, bootstrap.GameServer.TryAssignBusinessTypeAsync(node.lotId, node.businessTypeId));
	}
}

