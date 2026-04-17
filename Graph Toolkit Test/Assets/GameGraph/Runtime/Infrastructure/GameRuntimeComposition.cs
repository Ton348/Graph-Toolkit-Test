using Game1.Graph.Runtime.Infrastructure;

namespace GameGraph.Runtime.Infrastructure
{
	public static class GameRuntimeComposition
	{
		public static GameGraphComposition Create()
		{
			var executorRegistry = new GameGraphExecutorRegistry();

			GameRuntimeNodeConverterRegistration.Register();
			GameRuntimeExecutorRegistration.Register(executorRegistry);

			return new GameGraphComposition(executorRegistry);
		}
	}
}