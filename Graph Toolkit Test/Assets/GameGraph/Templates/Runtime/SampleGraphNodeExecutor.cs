using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

// Copy this file, rename class and generic type, then implement node behavior.
// Add [GameGraphNodeExecutor] if you want auto-registration.
public abstract class SampleGraphNodeExecutor : GraphNodeExecutor<SampleGraphNode>
{
	protected override UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(
		SampleGraphNode node,
		GraphExecutionContext context,
		CancellationToken cancellationToken)
	{
		Debug.Log(node.message ?? string.Empty);
		return UniTask.FromResult(GraphNodeExecutionResult.ContinueTo(node.nextNodeId));
	}
}
