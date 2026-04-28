using System.Threading;
using Cysharp.Threading.Tasks;
using Graph.Core.Runtime.Executors.Templates;
using Graph.Core.Runtime.Nodes.Server;

namespace Graph.Core.Runtime.Executors.Server
{
	public sealed class CheckpointNodeExecutor : CoreGraphSuccessFailNodeExecutor<CheckpointNode>
	{
		protected override UniTask<bool> EvaluateSuccessAsync(
			CheckpointNode node,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			if (context.CheckpointService == null)
			{
				return UniTask.FromResult(false);
			}

			return ExecuteWithServiceAsync(node, context, context.CheckpointService, cancellationToken);
		}

		private static async UniTask<bool> ExecuteWithServiceAsync(
			CheckpointNode node,
			GraphExecutionContext context,
			IGraphCheckpointService checkpointService,
			CancellationToken cancellationToken)
		{
			string graphId = context?.CurrentGraph != null ? context.CurrentGraph.name : null;
			return node.action == CheckpointAction.Clear
				? await checkpointService.ClearAsync(graphId, node.checkpointId, cancellationToken)
				: await checkpointService.SaveAsync(graphId, node.checkpointId, cancellationToken);
		}
	}
}
