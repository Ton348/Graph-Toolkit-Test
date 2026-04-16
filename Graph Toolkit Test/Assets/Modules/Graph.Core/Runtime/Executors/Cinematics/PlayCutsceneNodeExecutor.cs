using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Executors.Templates;
using GraphCore.Runtime.Nodes.Cinematics;

namespace GraphCore.Runtime.Executors.Cinematics
{
	public sealed class PlayCutsceneNodeExecutor : CoreGraphNextNodeExecutor<PlayCutsceneNode>
	{
		protected override UniTask<GraphNodeExecutionResult> ExecuteNodeAsync(
			PlayCutsceneNode node,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			if (context.CutsceneService == null)
			{
				return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
			}

			return ExecuteWithServiceAsync(node, context.CutsceneService, cancellationToken);
		}

		private static async UniTask<GraphNodeExecutionResult> ExecuteWithServiceAsync(
			PlayCutsceneNode node,
			IGraphCutsceneService cutsceneService,
			CancellationToken cancellationToken)
		{
			await cutsceneService.PlayAsync(node.cutsceneReference, cancellationToken);
			return GraphNodeExecutionResult.ContinueTo(node.nextNodeId);
		}
	}
}