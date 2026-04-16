using System.Threading;
using Cysharp.Threading.Tasks;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using Graph.Core.Runtime;

[GameGraphNodeExecutorAttribute]
public sealed class
	RequestHireBusinessWorkerNodeExecutor : GameGraphServerRequestExecutor<RequestHireBusinessWorkerNode>
{
	protected override UniTask<ServerActionResult> ExecuteRequestAsync(
		RequestHireBusinessWorkerNode node,
		GameBootstrap bootstrap,
		GraphExecutionContext context,
		CancellationToken cancellationToken)
	{
		return GameGraphExecutorContext.ExecuteServerAsync(context,
			bootstrap.GameServer.TryHireBusinessWorkerAsync(node.lotId, node.roleId, node.contactId));
	}
}