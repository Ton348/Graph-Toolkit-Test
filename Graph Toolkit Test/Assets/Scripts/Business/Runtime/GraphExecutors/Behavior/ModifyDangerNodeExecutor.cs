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
	public sealed class ModifyDangerNodeExecutor : BaseGraphNodeExecutor<ModifyDangerNode>
	{
		protected override async UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(ModifyDangerNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			if (context == null || !context.TryGet(GraphContextKeys.runtimeNpcBehaviorBridge, out var bridge) || bridge == null)
			{
				return GraphNodeExecutionResult.Fault("NPCBehaviorRuntimeBridge missing in graph context.", GraphNodeExecutionErrorType.ServiceFailure);
			}

			switch (node.mode)
			{
				case DangerModifyMode.EnableSpreadDanger:
					await bridge.SpreadDangerBurstsAsync(node.value, cancellationToken);
					break;
				case DangerModifyMode.SetDangerRadius:
					bridge.ModifyDangerRadius(node.value);
					break;
				case DangerModifyMode.SetDangerTimer:
					bridge.ModifyDangerTimer(node.value);
					break;
				case DangerModifyMode.SetThreatScore:
					bridge.ModifyThreatScore(Mathf.RoundToInt(node.value));
					break;
			}

			return GraphNodeExecutionResult.ContinueTo(node.nextNodeId);
		}
	}
}
