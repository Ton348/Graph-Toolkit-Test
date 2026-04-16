using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace GraphCore.Runtime
{
	public interface IGraphNodeExecutor
	{
		Type NodeType { get; }
		UniTask<GraphNodeExecutionResult> ExecuteAsync(BaseGraphNode node, GraphExecutionContext context, CancellationToken cancellationToken);
	}
}
