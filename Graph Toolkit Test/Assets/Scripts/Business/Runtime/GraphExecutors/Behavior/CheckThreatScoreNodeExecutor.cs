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
	public sealed class CheckThreatScoreNodeExecutor : BaseGraphNodeExecutor<CheckThreatScoreNode>
	{
		protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(CheckThreatScoreNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			if (context == null || !context.TryGet(GraphContextKeys.runtimeNpcBehaviorBridge, out var bridge) || bridge == null)
			{
				return UniTask.FromResult(GraphNodeExecutionResult.Fault("NPCBehaviorRuntimeBridge missing in graph context.", GraphNodeExecutionErrorType.ServiceFailure));
			}

			bool result = bridge.IsThreatMatch(node.valueA, node.valueB, node.compareType);
			return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(result ? node.trueNodeId : node.falseNodeId));
		}
	}
}
