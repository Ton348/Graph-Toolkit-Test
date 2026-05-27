using Cysharp.Threading.Tasks;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using Graph.Core.Runtime;
using Graph.Core.Runtime.Executors;
using Graph.Core.Runtime.Nodes.Behavior;
using Prototype.Business.NPC.Needs;
using Prototype.Business.Runtime;
using System.Threading;

namespace Prototype.Business.Runtime.GraphExecutors.Behavior
{
	[GameGraphNodeExecutor]
	public sealed class ModifyNeedNodeExecutor : BaseGraphNodeExecutor<ModifyNeedNode>
	{
		protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(ModifyNeedNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			if (context == null || !context.TryGet(GraphContextKeys.runtimeNpcBehaviorBridge, out var bridge) || bridge == null || bridge.Needs == null)
			{
				return UniTask.FromResult(GraphNodeExecutionResult.Fault("NPCBehaviorRuntimeBridge missing in graph context.", GraphNodeExecutionErrorType.ServiceFailure));
			}

			NPCNeedType needType = ConvertNeedType(node.needType);
			float amount = UnityEngine.Mathf.Abs(node.amount);
			if (node.increase)
			{
				bridge.Needs.AddNeed(needType, amount);
			}
			else
			{
				bridge.Needs.ConsumeNeed(needType, amount);
			}

			return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
		}

		private static NPCNeedType ConvertNeedType(BehaviorNeedType needType)
		{
			switch (needType)
			{
				case BehaviorNeedType.Thirst: return NPCNeedType.Thirst;
				case BehaviorNeedType.Toilet: return NPCNeedType.Toilet;
				case BehaviorNeedType.Fun: return NPCNeedType.Fun;
				case BehaviorNeedType.Energy: return NPCNeedType.Energy;
				default: return NPCNeedType.Hunger;
			}
		}
	}
}
