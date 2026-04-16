using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Nodes.Server;
using System.Threading;

namespace GraphCore.Runtime.Executors.Server
{
	public sealed class CheckpointNodeExecutor : BaseGraphNodeExecutor<CheckpointNode>
	{
		protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(CheckpointNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			if (context.CheckpointService == null)
			{
				return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.failNodeId));
			}

			return ExecuteWithServiceAsync(node, context.CheckpointService, cancellationToken);
		}

		private static async UniTask<GraphNodeExecutionResult> ExecuteWithServiceAsync(CheckpointNode node, IGraphCheckpointService checkpointService, CancellationToken cancellationToken)
		{
			bool success = node.action == CheckpointAction.Clear
				? await checkpointService.ClearAsync(node.checkpointId, cancellationToken)
				: await checkpointService.SaveAsync(node.checkpointId, cancellationToken);

			return GraphNodeExecutionResult.ContinueTo(success ? node.successNodeId : node.failNodeId);
		}
	}
}
