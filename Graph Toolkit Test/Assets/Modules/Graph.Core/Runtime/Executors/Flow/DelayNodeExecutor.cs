using System.Threading;
using Cysharp.Threading.Tasks;
using Graph.Core.Runtime.Executors.Templates;
using Graph.Core.Runtime.Nodes.Flow;
using UnityEngine;

namespace Graph.Core.Runtime.Executors.Flow
{
	public sealed class DelayNodeExecutor : CoreGraphNextNodeExecutor<DelayNode>
	{
		protected override async UniTask<GraphNodeExecutionResult> ExecuteNodeAsync(
			DelayNode node,
			GraphExecutionContext context,
			CancellationToken cancellationToken)
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