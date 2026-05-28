using Cysharp.Threading.Tasks;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using Graph.Core.Runtime;
using Graph.Core.Runtime.Executors;
using Graph.Core.Runtime.Nodes.Behavior;
using Prototype.Business.Runtime;
using System.Threading;

namespace Prototype.Business.Runtime.GraphExecutors.Behavior
{
	[GameGraphNodeExecutor]
	public sealed class HungerThresholdReachedNodeExecutor : BaseGraphNodeExecutor<HungerThresholdReachedNode>
	{
		protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(HungerThresholdReachedNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			if (context == null || !context.TryGet(GraphContextKeys.runtimeNpcBehaviorBridge, out var bridge) || bridge == null)
			{
				return UniTask.FromResult(GraphNodeExecutionResult.Fault("NPCBehaviorRuntimeBridge missing in graph context.", GraphNodeExecutionErrorType.ServiceFailure));
			}

			bool reached = bridge.GetNeedValue(node.needType) >= node.threshold;
			return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(reached ? node.trueNodeId : node.falseNodeId));
		}
	}
}
