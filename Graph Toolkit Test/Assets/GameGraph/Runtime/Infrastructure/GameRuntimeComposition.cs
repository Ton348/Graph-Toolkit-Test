public static class GameRuntimeComposition
{
	public static GameGraphComposition Create()
	{
		GameGraphExecutorRegistry executorRegistry = new GameGraphExecutorRegistry();

		GameRuntimeNodeConverterRegistration.Register();
		GameRuntimeExecutorRegistration.Register(executorRegistry);

		return new GameGraphComposition(executorRegistry);
	}
}
