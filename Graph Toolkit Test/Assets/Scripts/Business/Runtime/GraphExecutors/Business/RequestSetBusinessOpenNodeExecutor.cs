using System.Threading;
using Cysharp.Threading.Tasks;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using GraphCore.Runtime;

[GameGraphNodeExecutorAttribute]
public sealed class RequestSetBusinessOpenNodeExecutor : GameGraphServerRequestExecutor<RequestSetBusinessOpenNode>
{
	protected override UniTask<ServerActionResult> ExecuteRequestAsync(
		RequestSetBusinessOpenNode node,
		GameBootstrap bootstrap,
		GraphExecutionContext context,
		CancellationToken cancellationToken)
	{
		return node.open
			? GameGraphExecutorContext.ExecuteServerAsync(context,
				bootstrap.GameServer.TryOpenBusinessAsync(node.lotId))
			: GameGraphExecutorContext.ExecuteServerAsync(context,
				bootstrap.GameServer.TryCloseBusinessAsync(node.lotId));
	}
}