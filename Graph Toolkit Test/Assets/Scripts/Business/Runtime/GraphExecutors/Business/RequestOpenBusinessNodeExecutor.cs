using System.Threading;
using Cysharp.Threading.Tasks;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using GraphCore.Runtime;

[GameGraphNodeExecutorAttribute]
public sealed class RequestOpenBusinessNodeExecutor : GameGraphServerRequestExecutor<RequestOpenBusinessNode>
{
	protected override UniTask<ServerActionResult> ExecuteRequestAsync(
		RequestOpenBusinessNode node,
		GameBootstrap bootstrap,
		GraphExecutionContext context,
		CancellationToken cancellationToken)
	{
		return GameGraphExecutorContext.ExecuteServerAsync(context,
			bootstrap.GameServer.TryOpenBusinessAsync(node.lotId));
	}
}