using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Executors.Templates;
using GraphCore.Runtime.Nodes.Server;
using System.Threading;

namespace GraphCore.Runtime.Executors.Server
{
	public sealed class CompleteQuestNodeExecutor : CoreGraphSuccessFailNodeExecutor<CompleteQuestNode>
	{
		protected override UniTask<bool> EvaluateSuccessAsync(CompleteQuestNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			if (context.QuestService == null)
			{
				return UniTask.FromResult(false);
			}

			return ExecuteWithServiceAsync(node, context.QuestService, cancellationToken);
		}

		private static async UniTask<bool> ExecuteWithServiceAsync(CompleteQuestNode node, IGraphQuestService questService, CancellationToken cancellationToken)
		{
			return await questService.CompleteQuestAsync(node.questId, cancellationToken);
		}
	}
}
