using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Nodes.Server;
using System.Threading;
using GraphCore.Runtime;

namespace GraphCore.Runtime.Executors.Server
{
	public sealed class CompleteQuestNodeExecutor : BaseGraphNodeExecutor<CompleteQuestNode>
	{
		protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(CompleteQuestNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			if (context.QuestService == null)
			{
				return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.failNodeId));
			}

			return ExecuteWithServiceAsync(node, context.QuestService, cancellationToken);
		}

		private static async UniTask<GraphNodeExecutionResult> ExecuteWithServiceAsync(CompleteQuestNode node, IGraphQuestService questService, CancellationToken cancellationToken)
		{
			bool success = await questService.CompleteQuestAsync(node.questId, cancellationToken);
			return GraphNodeExecutionResult.ContinueTo(success ? node.successNodeId : node.failNodeId);
		}
	}
}
