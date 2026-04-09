public enum GraphNodeExecutionSignal
{
    Continue = 0,
    Stop = 1,
    Fault = 2
}

public enum GraphNodeExecutionErrorType
{
    None = 0,
    InvalidNode = 1,
    InvalidTransition = 2,
    ServiceFailure = 3,
    InternalError = 4
}

public readonly struct GraphNodeExecutionResult
{
    public readonly GraphNodeExecutionSignal signal;
    public readonly string nextNodeId;
    public readonly string diagnosticMessage;
    public readonly GraphNodeExecutionErrorType errorType;

    public GraphNodeExecutionResult(GraphNodeExecutionSignal signal, string nextNodeId, string diagnosticMessage, GraphNodeExecutionErrorType errorType)
    {
        this.signal = signal;
        this.nextNodeId = nextNodeId;
        this.diagnosticMessage = diagnosticMessage;
        this.errorType = errorType;
    }

    public static GraphNodeExecutionResult ContinueTo(string nextNodeId)
    {
        return new GraphNodeExecutionResult(GraphNodeExecutionSignal.Continue, nextNodeId, null, GraphNodeExecutionErrorType.None);
    }

    public static GraphNodeExecutionResult Stop(string message = null)
    {
        return new GraphNodeExecutionResult(GraphNodeExecutionSignal.Stop, null, message, GraphNodeExecutionErrorType.None);
    }

    public static GraphNodeExecutionResult Fault(string message, GraphNodeExecutionErrorType errorType = GraphNodeExecutionErrorType.InternalError)
    {
        return new GraphNodeExecutionResult(GraphNodeExecutionSignal.Fault, null, message, errorType);
    }

    public bool IsSuccess => signal == GraphNodeExecutionSignal.Continue;
    public bool IsFault => signal == GraphNodeExecutionSignal.Fault;
    public bool IsStop => signal == GraphNodeExecutionSignal.Stop;
}
