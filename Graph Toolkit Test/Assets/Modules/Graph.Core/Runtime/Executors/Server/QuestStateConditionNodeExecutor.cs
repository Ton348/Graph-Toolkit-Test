using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Executors.Templates;
using GraphCore.Runtime.Nodes.Server;

namespace GraphCore.Runtime.Executors.Server
{
	public sealed class QuestStateConditionNodeExecutor : CoreGraphTrueFalseNodeExecutor<QuestStateConditionNode>
	{
		protected override UniTask<bool> EvaluateConditionAsync(
			QuestStateConditionNode node,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			if (context.QuestService == null)
			{
				return UniTask.FromResult(false);
			}

			return ExecuteWithServiceAsync(node, context.QuestService, cancellationToken);
		}

		private static async UniTask<bool> ExecuteWithServiceAsync(
			QuestStateConditionNode node,
			IGraphQuestService questService,
			CancellationToken cancellationToken)
		{
			QuestState actualState = await questService.GetQuestStateAsync(node.questId, cancellationToken);
			return actualState == node.state;
		}
	}
}