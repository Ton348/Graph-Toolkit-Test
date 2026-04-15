using System.Collections.Generic;
using System;

namespace Game1.Graph.Runtime
{
	public sealed class GameGraphExecutorRegistry
	{
		private readonly List<IGraphNodeExecutor> m_executors = new List<IGraphNodeExecutor>();
		private readonly Dictionary<Type, IGraphNodeExecutor> m_executorsByNodeType = new Dictionary<Type, IGraphNodeExecutor>();

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
