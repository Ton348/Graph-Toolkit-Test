using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[GameGraphNodeExecutor]
[MovedFrom(true, sourceNamespace: "", sourceAssembly: "Game1.Graph.Runtime", sourceClassName: "TestLogNodeExecutor")]
public sealed class TestLogNodeExecutor : GraphNodeExecutor<TestLogNode>
{
	protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(
		TestLogNode node,
		GraphExecutionContext context,
		CancellationToken cancellationToken)
	{
		Debug.Log(node.message ?? string.Empty);
		return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
	}
}
