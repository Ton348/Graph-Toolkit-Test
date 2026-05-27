using Cysharp.Threading.Tasks;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
using Graph.Core.Runtime;
using Graph.Core.Runtime.Executors;
using Graph.Core.Runtime.Nodes.Behavior;
using Prototype.Business.Runtime;
using Prototype.Business.Time;
using System.Threading;

namespace Prototype.Business.Runtime.GraphExecutors.Behavior
{
	[GameGraphNodeExecutor]
	public sealed class SelectGoalNodeExecutor : BaseGraphNodeExecutor<SelectGoalNode>
	{
		protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(SelectGoalNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			if (context == null || !context.TryGet(GraphContextKeys.runtimeNpcBehaviorBridge, out var bridge) || bridge == null)
			{
				return UniTask.FromResult(GraphNodeExecutionResult.Fault("NPCBehaviorRuntimeBridge missing in graph context.", GraphNodeExecutionErrorType.ServiceFailure));
			}
			bridge.DebugActiveBehaviorNode = "SelectGoal";

			WorldTimeSystem worldTime = WorldTimeSystem.Instance;
			if (worldTime != null && worldTime.IsNight())
			{
				return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.goHomeNodeId));
			}

			float needValue = bridge.GetNeedValue(node.needType);
			if (needValue >= node.needThreshold)
			{
				return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.goFoodNodeId));
			}

			if (worldTime != null && worldTime.IsBusinessHours(node.workHourFrom, node.workHourTo))
			{
				return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.goWorkNodeId));
			}

			return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.wanderNodeId));
		}
	}
}
