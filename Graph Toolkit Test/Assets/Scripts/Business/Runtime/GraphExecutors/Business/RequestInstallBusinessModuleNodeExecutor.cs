using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Game1.Graph.Runtime;

[GameGraphNodeExecutor]
public sealed class RequestInstallBusinessModuleNodeExecutor : GameGraphServerRequestExecutor<RequestInstallBusinessModuleNode>
{
	protected override UniTask<ServerActionResult> ExecuteRequestAsync(RequestInstallBusinessModuleNode node, GameBootstrap bootstrap, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		return GameGraphExecutorContext.ExecuteServerAsync(context, bootstrap.GameServer.TryInstallBusinessModuleAsync(node.lotId, node.moduleId));
	}
}

