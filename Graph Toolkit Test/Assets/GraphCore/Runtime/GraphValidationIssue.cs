public enum GraphValidationSeverity
{
    Warning = 0,
    Error = 1
}

public sealed class GraphValidationIssue
{
    public GraphValidationIssue(
        GraphValidationSeverity severity,
        string message,
        string nodeId = null,
        string nodeType = null,
        string fieldName = null,
        int nodeIndex = -1)
    {
        this.severity = severity;
        this.message = message;
        this.nodeId = nodeId;
        this.nodeType = nodeType;
        this.fieldName = fieldName;
        this.nodeIndex = nodeIndex;
    }

    public GraphValidationSeverity severity { get; }
    public string message { get; }
    public string nodeId { get; }
    public string nodeType { get; }
    public string fieldName { get; }
    public int nodeIndex { get; }

    public bool IsError => severity == GraphValidationSeverity.Error;

    public override string ToString()
    {
        string location = string.IsNullOrWhiteSpace(nodeId) ? string.Empty : $" nodeId='{nodeId}'";
        string type = string.IsNullOrWhiteSpace(nodeType) ? string.Empty : $" nodeType='{nodeType}'";
        string field = string.IsNullOrWhiteSpace(fieldName) ? string.Empty : $" field='{fieldName}'";
        string index = nodeIndex >= 0 ? $" index={nodeIndex}" : string.Empty;

        string prefix = severity == GraphValidationSeverity.Error ? "ERROR" : "WARNING";
        return $"[{prefix}] {message}{location}{type}{field}{index}";
    }

    public static GraphValidationIssue Error(string message, string nodeId = null, string nodeType = null, string fieldName = null, int nodeIndex = -1)
    {
        return new GraphValidationIssue(GraphValidationSeverity.Error, message, nodeId, nodeType, fieldName, nodeIndex);
    }

    public static GraphValidationIssue Warning(string message, string nodeId = null, string nodeType = null, string fieldName = null, int nodeIndex = -1)
    {
        return new GraphValidationIssue(GraphValidationSeverity.Warning, message, nodeId, nodeType, fieldName, nodeIndex);
    }
}
