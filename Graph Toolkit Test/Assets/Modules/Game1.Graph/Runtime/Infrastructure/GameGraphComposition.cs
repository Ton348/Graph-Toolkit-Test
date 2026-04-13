using System;
using System.Collections.Generic;

public sealed class GameGraphComposition
{
	private readonly GameGraphExecutorRegistry m_executorRegistry;

	public GameGraphComposition(GameGraphExecutorRegistry executorRegistry)
	{
		m_executorRegistry = executorRegistry ?? throw new ArgumentNullException(nameof(executorRegistry));
	}

	public GameGraphExecutorRegistry ExecutorRegistry => m_executorRegistry;

	public GraphNodeExecutorRegistry CreateRuntimeExecutorRegistry()
	{
		List<IGraphNodeExecutor> executors = new List<IGraphNodeExecutor>();
		executors.AddRange(CommonGraphRuntimeComposition.CreateDefaultExecutors());
		executors.AddRange(m_executorRegistry.GetExecutors());
		return CommonGraphRuntimeComposition.CreateRegistry(executors);
	}

	public static GameGraphComposition CreateDefault()
	{
		return new GameGraphComposition(new GameGraphExecutorRegistry());
	}
}
