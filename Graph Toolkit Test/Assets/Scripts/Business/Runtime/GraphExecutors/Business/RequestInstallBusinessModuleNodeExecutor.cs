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
	public sealed class
		RequestInstallBusinessModuleNodeExecutor : GameGraphServerRequestExecutor<RequestInstallBusinessModuleNode>
	{
		protected override UniTask<ServerActionResult> ExecuteRequestAsync(
			RequestInstallBusinessModuleNode node,
			GameBootstrap bootstrap,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			return GameGraphExecutorContext.ExecuteServerAsync(context,
				bootstrap.GameServer.TryInstallBusinessModuleAsync(node.lotId, node.moduleId));
		}
	}
}