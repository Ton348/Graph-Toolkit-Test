using System;
using System.Collections.Generic;

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

		if (m_executorsByNodeType.TryGetValue(executor.NodeType, out IGraphNodeExecutor existingExecutor))
		{
			throw new InvalidOperationException(
				$"Executor for node type '{executor.NodeType.Name}' is already registered by '{existingExecutor.GetType().Name}'.");
		}

		m_executors.Add(executor);
		m_executorsByNodeType.Add(executor.NodeType, executor);
	}

	public IReadOnlyList<IGraphNodeExecutor> GetExecutors()
	{
		return m_executors;
	}
}
