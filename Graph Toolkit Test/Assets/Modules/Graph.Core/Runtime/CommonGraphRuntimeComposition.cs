using System;
using System.Collections.Generic;
using GraphCore.Runtime.Executors.Cinematics;
using GraphCore.Runtime.Executors.Flow;
using GraphCore.Runtime.Executors.Server;
using GraphCore.Runtime.Executors.UI;
using GraphCore.Runtime.Executors.Utility;
using GraphCore.Runtime.Executors.World;

namespace GraphCore.Runtime
{
	public static class CommonGraphRuntimeComposition
	{
		public static IReadOnlyList<IGraphNodeExecutor> CreateDefaultExecutors()
		{
			return new List<IGraphNodeExecutor>
			{
				new StartNodeExecutor(),
				new FinishNodeExecutor(),
				new LogNodeExecutor(),
				new DelayNodeExecutor(),
				new RandomNodeExecutor(),
				new DialogueNodeExecutor(),
				new ChoiceNodeExecutor(),
				new MapMarkerNodeExecutor(),
				new PlayCutsceneNodeExecutor(),
				new CheckpointNodeExecutor(),
				new StartQuestNodeExecutor(),
				new CompleteQuestNodeExecutor(),
				new QuestStateConditionNodeExecutor()
			};
		}

		public static GraphNodeExecutorRegistry CreateRegistry(IEnumerable<IGraphNodeExecutor> executors)
		{
			if (executors == null)
			{
				throw new ArgumentNullException(nameof(executors));
			}

			return new GraphNodeExecutorRegistry(executors);
		}

		public static GraphNodeExecutorRegistry CreateDefaultRegistry()
		{
			return CreateRegistry(CreateDefaultExecutors());
		}
	}
}
