using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

[GameGraphNodeExecutor]
public sealed class CheckBusinessOpenNodeExecutor : GameGraphTrueFalseNodeExecutor<CheckBusinessOpenNode>
{
	protected override UniTask<bool> EvaluateConditionAsync(CheckBusinessOpenNode node, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		bool result = GameGraphExecutorContext.TryGetBootstrap(context, out GameBootstrap bootstrap)
			&& bootstrap.BusinessStateSyncService != null
			&& bootstrap.BusinessStateSyncService.IsBusinessOpen(node.lotId);
		return UniTask.FromResult(result);
	}
}

