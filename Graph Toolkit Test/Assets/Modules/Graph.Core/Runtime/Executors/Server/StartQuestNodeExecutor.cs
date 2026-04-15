using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Nodes.Server;
using System.Threading;
using GraphCore.Runtime;

namespace GraphCore.Runtime.Executors.Server
{
	public sealed class StartQuestNodeExecutor : BaseGraphNodeExecutor<StartQuestNode>
	{
		protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(StartQuestNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			if (context.QuestService == null)
			{
				return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.failNodeId));
			}

			return ExecuteWithServiceAsync(node, context.QuestService, cancellationToken);
		}

		private static async UniTask<GraphNodeExecutionResult> ExecuteWithServiceAsync(StartQuestNode node, IGraphQuestService questService, CancellationToken cancellationToken)
		{
			bool success = await questService.StartQuestAsync(node.questId, cancellationToken);
			return GraphNodeExecutionResult.ContinueTo(success ? node.successNodeId : node.failNodeId);
		}
	}
}
