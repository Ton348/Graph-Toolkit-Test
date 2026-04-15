using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using GraphCore.Runtime;

namespace GraphCore.Runtime.Executors
{
	public abstract class BaseGraphNodeExecutor<TNode> : IGraphNodeExecutor where TNode : BaseGraphNode
	{
		public Type NodeType => typeof(TNode);

		public UniTask<GraphNodeExecutionResult> ExecuteAsync(BaseGraphNode node, GraphExecutionContext context, CancellationToken cancellationToken)
		{
			if (context == null)
			{
				return UniTask.FromResult(GraphNodeExecutionResult.Fault($"Graph execution context is null for executor {GetType().Name}."));
			}
			if (node == null)
			{
				return UniTask.FromResult(GraphNodeExecutionResult.Fault($"Graph node is null for executor {GetType().Name}."));
			}
			if (node is not TNode typedNode)
			{
				string actualType = node.GetType().Name;
				string message = $"Node type mismatch for executor {GetType().Name}. Expected {typeof(TNode).Name}, got {actualType}.";
				return UniTask.FromResult(GraphNodeExecutionResult.Fault(message));
			}

			return ExecuteTypedAsync(typedNode, context, cancellationToken);
		}

		protected abstract UniTask<GraphNodeExecutionResult> ExecuteTypedAsync(TNode node, GraphExecutionContext context, CancellationToken cancellationToken);
	}
}
