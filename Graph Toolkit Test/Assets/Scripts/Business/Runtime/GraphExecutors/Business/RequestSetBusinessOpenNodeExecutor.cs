using System.Threading;
using Cysharp.Threading.Tasks;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using GameGraph.Runtime.Business;
using Graph.Core.Runtime;
using Prototype.Business.Bootstrap;
using Prototype.Business.Runtime.GraphExecutors.Infrastructure;
using Prototype.Business.Services;

namespace Prototype.Business.Runtime.GraphExecutors.Business
{
	[GameGraphNodeExecutor]
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
}