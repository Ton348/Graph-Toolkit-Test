using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Game1.Graph.Runtime;

[GameGraphNodeExecutor]
public sealed class CheckBusinessModuleInstalledNodeExecutor : GameGraphTrueFalseNodeExecutor<CheckBusinessModuleInstalledNode>
{
	protected override UniTask<bool> EvaluateConditionAsync(CheckBusinessModuleInstalledNode node, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		bool result = GameGraphExecutorContext.TryGetBootstrap(context, out GameBootstrap bootstrap)
			&& bootstrap.BusinessStateSyncService != null
			&& bootstrap.BusinessStateSyncService.HasModule(node.lotId, node.moduleId);
		return UniTask.FromResult(result);
	}
}

