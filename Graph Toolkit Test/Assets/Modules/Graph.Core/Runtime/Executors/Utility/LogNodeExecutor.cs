using Cysharp.Threading.Tasks;
using GraphCore.BaseNodes.Runtime.Utility;
using System.Threading;

public sealed class LogNodeExecutor : BaseGraphNodeExecutor<LogNode>
{
	protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(LogNode node, GraphExecutionContext context, CancellationToken cancellationToken)
	{
		return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
	}
}
