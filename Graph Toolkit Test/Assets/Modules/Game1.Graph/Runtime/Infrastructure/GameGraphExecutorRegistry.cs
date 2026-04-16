using System;
using System.Collections.Generic;
using GraphCore.Runtime;

namespace Game1.Graph.Runtime.Infrastructure
{
	public sealed class GameGraphExecutorRegistry
	{
		private readonly List<IGraphNodeExecutor> m_executors = new();
		private readonly Dictionary<Type, IGraphNodeExecutor> m_executorsByNodeType = new();

		public void Register(IGraphNodeExecutor executor)
		{
			if (executor == null)
			{
				throw new ArgumentNullException(nameof(executor));
			}

			if (executor.NodeType == null)
			{
				throw new InvalidOperationException($"Executor '{executor.GetType().Name}' has null NodeType.");
			}

			if (m_executorsByNodeType.TryGetValue(executor.NodeType, out IGraphNodeExecutor existingExecutor))
			{
				// replace existing (deterministic behavior)
				m_executors.Remove(existingExecutor);
				m_executorsByNodeType[executor.NodeType] = executor;
				m_executors.Add(executor);
				return;
			}

			m_executors.Add(executor);
			m_executorsByNodeType.Add(executor.NodeType, executor);
		}

		public void Register<TExecutor>() where TExecutor : IGraphNodeExecutor, new()
		{
			Register(new TExecutor());
		}

		public bool TryGetExecutor(Type nodeType, out IGraphNodeExecutor executor)
		{
			if (nodeType == null)
			{
				executor = null;
				return false;
			}

			return m_executorsByNodeType.TryGetValue(nodeType, out executor);
		}

		public IReadOnlyList<IGraphNodeExecutor> GetExecutors()
		{
			return m_executors.AsReadOnly();
		}
	}
}