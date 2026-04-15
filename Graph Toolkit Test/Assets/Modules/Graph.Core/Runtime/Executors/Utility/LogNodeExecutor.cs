using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Nodes.Utility;
using System.Threading;
using GraphCore.Runtime;

namespace GraphCore.Runtime.Executors.Utility
{
	public sealed class LogNodeExecutor : BaseGraphNodeExecutor<LogNode>
	{
		protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(LogNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
		}
	}
}
