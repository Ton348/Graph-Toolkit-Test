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
	public sealed class ModifyNeedNodeExecutor : BaseGraphNodeExecutor<ModifyNeedNode>
	{
		protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(ModifyNeedNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			if (context == null || !context.TryGet(GraphContextKeys.runtimeNpcBehaviorBridge, out var bridge) || bridge == null || bridge.Needs == null)
			{
				return UniTask.FromResult(GraphNodeExecutionResult.Fault("NPCBehaviorRuntimeBridge missing in graph context.", GraphNodeExecutionErrorType.ServiceFailure));
			}

			bridge.ModifyNeedValue(node.needType, node.amount, node.increase);

			return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
		}
	}
}
