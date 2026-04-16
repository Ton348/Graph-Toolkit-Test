using System.Threading;
using Cysharp.Threading.Tasks;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using GraphCore.Runtime;

[GameGraphNodeExecutorAttribute]
public sealed class RequestSetBusinessMarkupNodeExecutor : GameGraphServerRequestExecutor<RequestSetBusinessMarkupNode>
{
	protected override UniTask<ServerActionResult> ExecuteRequestAsync(
		RequestSetBusinessMarkupNode node,
		GameBootstrap bootstrap,
		GraphExecutionContext context,
		CancellationToken cancellationToken)
	{
		return GameGraphExecutorContext.ExecuteServerAsync(context,
			bootstrap.GameServer.TrySetBusinessMarkupAsync(node.lotId, node.markupPercent));
	}
}