using Cysharp.Threading.Tasks;
using GraphCore.BaseNodes.Runtime.Cinematics;
using System.Threading;

public sealed class PlayCutsceneNodeExecutor : BaseGraphNodeExecutor<PlayCutsceneNode>
{
	protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(PlayCutsceneNode node, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		if (context.CutsceneService == null)
		{
			return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
		}

		return ExecuteWithServiceAsync(node, context.CutsceneService, cancellationToken);
	}

	private static async UniTask<GraphNodeExecutionResult> ExecuteWithServiceAsync(PlayCutsceneNode node, IGraphCutsceneService cutsceneService, CancellationToken cancellationToken)
	{
		await cutsceneService.PlayAsync(node.cutsceneReference, cancellationToken);
		return GraphNodeExecutionResult.ContinueTo(node.nextNodeId);
	}
}
