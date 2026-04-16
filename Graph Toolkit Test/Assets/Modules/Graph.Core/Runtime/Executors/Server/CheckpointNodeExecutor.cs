using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Executors.Templates;
using GraphCore.Runtime.Nodes.Server;

namespace GraphCore.Runtime.Executors.Server
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

			return ExecuteWithServiceAsync(node, context.CheckpointService, cancellationToken);
		}

		private static async UniTask<bool> ExecuteWithServiceAsync(
			CheckpointNode node,
			IGraphCheckpointService checkpointService,
			CancellationToken cancellationToken)
		{
			return node.action == CheckpointAction.Clear
				? await checkpointService.ClearAsync(node.checkpointId, cancellationToken)
				: await checkpointService.SaveAsync(node.checkpointId, cancellationToken);
		}
	}
}