namespace GraphCore.Runtime
{
	public enum GraphNodeExecutionErrorType
	{
		None = 0,
		InvalidNode = 1,
		InvalidTransition = 2,
		ServiceFailure = 3,
		InternalError = 4
	}
}
