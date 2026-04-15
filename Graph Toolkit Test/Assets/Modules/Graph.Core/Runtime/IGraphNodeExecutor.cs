using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using GraphCore.Runtime;

namespace GraphCore.Runtime
{
	public interface IGraphNodeExecutor
	{
		Type NodeType { get; }
		UniTask<GraphNodeExecutionResult> ExecuteAsync(BaseGraphNode node, GraphExecutionContext context, CancellationToken cancellationToken);
	}
}
