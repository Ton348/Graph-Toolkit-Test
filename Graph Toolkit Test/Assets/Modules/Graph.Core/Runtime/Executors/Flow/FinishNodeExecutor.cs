using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Nodes.Flow;

namespace GraphCore.Runtime.Executors.Flow
{
	public sealed class FinishNodeExecutor : BaseGraphNodeExecutor<FinishNode>
	{
		protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(
			FinishNode node,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
		{
			context?.DialogueService?.EndConversation();
			return UniTask.FromResult(GraphNodeExecutionResult.Stop("Finish node reached."));
		}
	}
}