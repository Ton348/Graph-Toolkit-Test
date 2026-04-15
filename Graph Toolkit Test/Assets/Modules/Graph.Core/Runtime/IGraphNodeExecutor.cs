using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public interface IGraphNodeExecutor
{
	Type NodeType { get; }
	UniTask<GraphNodeExecutionResult> ExecuteAsync(BaseGraphNode node, GraphExecutionContext context, CancellationToken cancellationToken);
}
