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
		protected override async UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(HungerThresholdReachedNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			if (context == null || !context.TryGet(GraphContextKeys.runtimeNpcBehaviorBridge, out var bridge) || bridge == null)
			{
				return GraphNodeExecutionResult.Fault("NPCBehaviorRuntimeBridge missing in graph context.", GraphNodeExecutionErrorType.ServiceFailure);
			}

			await bridge.WaitForNeedThresholdAsync(node.needType, node.threshold, cancellationToken);
			return GraphNodeExecutionResult.ContinueTo(node.nextNodeId);
		}
	}
}
