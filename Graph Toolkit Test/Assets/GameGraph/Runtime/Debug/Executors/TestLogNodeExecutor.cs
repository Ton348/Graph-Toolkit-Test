using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting.APIUpdating;

[GameGraphNodeExecutor]
[MovedFrom(true, sourceNamespace: "", sourceAssembly: "Game1.Graph.Runtime", sourceClassName: "TestLogNodeExecutor")]
public sealed class TestLogNodeExecutor : BaseGraphNodeExecutor<TestLogNode>
{
	protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(
		TestLogNode node,
		GraphExecutionContext context,
		CancellationToken cancellationToken)
	{
		return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
	}
}
