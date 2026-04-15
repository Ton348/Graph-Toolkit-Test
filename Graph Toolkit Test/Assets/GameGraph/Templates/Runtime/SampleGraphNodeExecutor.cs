using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.Runtime;
using GraphCore.Runtime.Executors;
using Game1.Graph.Runtime;

using Game1.Graph.Runtime.Infrastructure;
// Copy this file, rename class and generic type, then implement node behavior.
// Add [GameGraphNodeExecutor] if you want auto-registration.
public abstract class SampleGraphNodeExecutor : BaseGraphNodeExecutor<SampleGraphNode>
{
	protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(
		SampleGraphNode node,
		GraphExecutionContext context,
		CancellationToken cancellationToken)
	{
		return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
	}
}
