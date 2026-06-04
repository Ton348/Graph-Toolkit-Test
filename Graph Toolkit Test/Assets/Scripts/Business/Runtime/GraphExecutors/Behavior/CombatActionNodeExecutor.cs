using Cysharp.Threading.Tasks;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using Graph.Core.Runtime;
using Graph.Core.Runtime.Executors;
using Graph.Core.Runtime.Nodes.Behavior;
using Prototype.Business.Runtime;
using System.Threading;
using UnityEngine;

namespace Prototype.Business.Runtime.GraphExecutors.Behavior
{
	[GameGraphNodeExecutor]
	public sealed class CombatActionNodeExecutor : BaseGraphNodeExecutor<CombatActionNode>
	{
		protected override async UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(CombatActionNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			if (context == null || !context.TryGet(GraphContextKeys.runtimeNpcBehaviorBridge, out var bridge) || bridge == null)
			{
				return GraphNodeExecutionResult.Fault("NPCBehaviorRuntimeBridge missing in graph context.", GraphNodeExecutionErrorType.ServiceFailure);
			}

			if (context.TryGet(GraphContextKeys.runtimePlayerTransform, out Transform playerTransform))
			{
				bridge.SetCombatContext(playerTransform);
			}

			bool success = await bridge.ExecuteCombatActionAsync(node.actionType, node.value, cancellationToken);
			return GraphNodeExecutionResult.ContinueTo(success ? node.successNodeId : node.failNodeId);
		}
	}
}
