using System.Collections.Generic;
using System;

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
