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
	public sealed class CheckCombatValueNodeExecutor : BaseGraphNodeExecutor<CheckCombatValueNode>
	{
		protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(CheckCombatValueNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			if (context == null || !context.TryGet(GraphContextKeys.runtimeNpcBehaviorBridge, out var bridge) || bridge == null)
			{
				return UniTask.FromResult(GraphNodeExecutionResult.Fault("NPCBehaviorRuntimeBridge missing in graph context.", GraphNodeExecutionErrorType.ServiceFailure));
			}

			if (context.TryGet(GraphContextKeys.runtimePlayerTransform, out Transform playerTransform))
			{
				bridge.SetCombatContext(playerTransform);
			}

			bool result = bridge.IsCombatValueMatch(node.valueType, node.comparisonType, node.value);
			return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(result ? node.trueNodeId : node.falseNodeId));
		}
	}
}
