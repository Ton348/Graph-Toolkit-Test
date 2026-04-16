using System.Threading;
using Cysharp.Threading.Tasks;
using Graph.Core.Runtime.Executors.Templates;
using Graph.Core.Runtime.Nodes.Server;

namespace Graph.Core.Runtime.Executors.Server
{
	public sealed class StartQuestNodeExecutor : CoreGraphSuccessFailNodeExecutor<StartQuestNode>
	{
		protected override UniTask<bool> EvaluateSuccessAsync(
			StartQuestNode node,
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
			StartQuestNode node,
			IGraphQuestService questService,
			CancellationToken cancellationToken)
		{
			return await questService.StartQuestAsync(node.questId, cancellationToken);
		}
	}
}