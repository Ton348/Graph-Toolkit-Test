using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

[GameGraphNodeExecutor]
public sealed class CheckBusinessExistsNodeExecutor : GameGraphTrueFalseNodeExecutor<CheckBusinessExistsNode>
{
	protected override UniTask<bool> EvaluateConditionAsync(CheckBusinessExistsNode node, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		bool result = GameGraphExecutorContext.TryGetBootstrap(context, out GameBootstrap bootstrap)
			&& bootstrap.BusinessStateSyncService != null
			&& bootstrap.BusinessStateSyncService.HasBusiness(node.lotId);
		return UniTask.FromResult(result);
	}
}

