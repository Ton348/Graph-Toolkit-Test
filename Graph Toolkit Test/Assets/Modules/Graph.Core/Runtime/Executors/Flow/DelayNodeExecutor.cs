using Cysharp.Threading.Tasks;
using GraphCore.Runtime.Nodes.Flow;
using System.Threading;
using UnityEngine;

namespace GraphCore.Runtime.Executors.Flow
{
	public sealed class DelayNodeExecutor : BaseGraphNodeExecutor<DelayNode>
	{
		protected override async UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(DelayNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			if (node.delaySeconds <= 0f)
			{
				return GraphNodeExecutionResult.ContinueTo(node.nextNodeId);
			}

			int delayMs = Mathf.Max(1, Mathf.RoundToInt(node.delaySeconds * 1000f));
			await UniTask.Delay(delayMs, cancellationToken: cancellationToken);
			return GraphNodeExecutionResult.ContinueTo(node.nextNodeId);
		}
	}
}
