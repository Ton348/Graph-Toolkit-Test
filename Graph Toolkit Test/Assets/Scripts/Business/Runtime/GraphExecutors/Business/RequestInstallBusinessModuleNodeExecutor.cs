using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.Runtime;
using UnityEngine;
using Game1.Graph.Runtime;

using Game1.Graph.Runtime.Infrastructure;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
[GameGraphNodeExecutorAttribute]
public sealed class RequestInstallBusinessModuleNodeExecutor : GameGraphServerRequestExecutor<RequestInstallBusinessModuleNode>
{
	protected override UniTask<ServerActionResult> ExecuteRequestAsync(RequestInstallBusinessModuleNode node, GameBootstrap bootstrap, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		return GameGraphExecutorContext.ExecuteServerAsync(context, bootstrap.GameServer.TryInstallBusinessModuleAsync(node.lotId, node.moduleId));
	}
}

