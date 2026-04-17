using System;
using System.Collections.Generic;
using Graph.Core.Runtime;

namespace Game1.Graph.Runtime.Infrastructure
{
	public sealed class GameGraphComposition
	{
		public GameGraphComposition(GameGraphExecutorRegistry executorRegistry)
		{
			ExecutorRegistry = executorRegistry ?? throw new ArgumentNullException(nameof(executorRegistry));
		}

		public GameGraphExecutorRegistry ExecutorRegistry { get; }

		public GraphNodeExecutorRegistry CreateRuntimeExecutorRegistry()
		{
			var executors = new List<IGraphNodeExecutor>();
			executors.AddRange(CommonGraphRuntimeComposition.CreateDefaultExecutors());
			executors.AddRange(ExecutorRegistry.GetExecutors());
			return CommonGraphRuntimeComposition.CreateRegistry(executors);
		}

		public void RegisterExecutor<TExecutor>() where TExecutor : IGraphNodeExecutor, new()
		{
			ExecutorRegistry.Register<TExecutor>();
		}

		public void RegisterExecutor(IGraphNodeExecutor executor)
		{
			ExecutorRegistry.Register(executor);
		}

		public static GameGraphComposition CreateDefault()
		{
			return new GameGraphComposition(new GameGraphExecutorRegistry());
		}
	}
}