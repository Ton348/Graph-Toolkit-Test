using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Graph.Core.Runtime
{
	public interface IGraphNodeExecutor
	{
		Type NodeType { get; }

		UniTask<GraphNodeExecutionResult> ExecuteAsync(
			BaseGraphNode node,
			GraphExecutionContext context,
			CancellationToken cancellationToken);
	}
}