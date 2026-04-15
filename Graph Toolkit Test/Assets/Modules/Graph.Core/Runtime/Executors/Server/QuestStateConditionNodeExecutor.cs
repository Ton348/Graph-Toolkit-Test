using Cysharp.Threading.Tasks;
using GraphCore.BaseNodes.Runtime.Server;
using System.Threading;

public sealed class QuestStateConditionNodeExecutor : BaseGraphNodeExecutor<QuestStateConditionNode>
{
	protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(QuestStateConditionNode node, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		if (context.QuestService == null)
		{
			return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.falseNodeId));
		}

		return ExecuteWithServiceAsync(node, context.QuestService, cancellationToken);
	}

	private static async UniTask<GraphNodeExecutionResult> ExecuteWithServiceAsync(QuestStateConditionNode node, IGraphQuestService questService, CancellationToken cancellationToken)
	{
		QuestState actualState = await questService.GetQuestStateAsync(node.questId, cancellationToken);
		bool isMatch = actualState == node.state;
		return GraphNodeExecutionResult.ContinueTo(isMatch ? node.trueNodeId : node.falseNodeId);
	}
}
