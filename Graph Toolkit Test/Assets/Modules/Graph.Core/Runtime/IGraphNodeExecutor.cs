using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public interface IGraphNodeExecutor
{
	Type NodeType { get; }
	UniTask<GraphNodeExecutionResult> ExecuteAsync(BaseGraphNode node, GraphExecutionContext context, CancellationToken cancellationToken);
}
