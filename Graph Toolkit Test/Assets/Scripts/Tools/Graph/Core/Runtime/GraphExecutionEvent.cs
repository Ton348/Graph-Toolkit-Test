using System;

public enum GraphExecutionEventType
{
    Start,
    Success,
    Fail
}

public sealed class GraphExecutionEvent
{
    public string GraphId { get; }
    public string NodeId { get; }
    public string NodeName { get; }
    public string NextNodeId { get; }
    public GraphExecutionEventType EventType { get; }
    public float Timestamp { get; }
    public string Message { get; }
    public ServerActionResult ServerResult { get; }
    public GraphContextSnapshot ContextSnapshot { get; }

    public GraphExecutionEvent(
        string graphId,
        string nodeId,
        string nodeName,
        string nextNodeId,
        GraphExecutionEventType eventType,
        float timestamp,
        string message,
        ServerActionResult serverResult,
        GraphContextSnapshot contextSnapshot)
    {
        GraphId = graphId;
        NodeId = nodeId;
        NodeName = nodeName;
        NextNodeId = nextNodeId;
        EventType = eventType;
        Timestamp = timestamp;
        Message = message;
        ServerResult = serverResult;
        ContextSnapshot = contextSnapshot;
    }
}

public sealed class GraphContextSnapshot
{
    public int? ChoiceLastIndex { get; }
    public string ChoiceLastLabel { get; }
    public bool? ConditionLastResult { get; }
    public string BuildingLastRequestedId { get; }
    public string QuestLastRequestedId { get; }
    public ServerActionResult ServerLastResult { get; }

    private GraphContextSnapshot(
        int? choiceLastIndex,
        string choiceLastLabel,
        bool? conditionLastResult,
        string buildingLastRequestedId,
        string questLastRequestedId,
        ServerActionResult serverLastResult)
    {
        ChoiceLastIndex = choiceLastIndex;
        ChoiceLastLabel = choiceLastLabel;
        ConditionLastResult = conditionLastResult;
        BuildingLastRequestedId = buildingLastRequestedId;
        QuestLastRequestedId = questLastRequestedId;
        ServerLastResult = serverLastResult;
    }

    public static GraphContextSnapshot Capture(GraphExecutionContext context)
    {
        if (context == null)
        {
            return null;
        }

        int? choiceIndex = null;
        if (context.TryGet(GraphContextKeys.ChoiceLastIndex, out int choiceValue))
        {
            choiceIndex = choiceValue;
        }

        string choiceLabel = null;
        context.TryGet(GraphContextKeys.ChoiceLastLabel, out choiceLabel);

        bool? conditionResult = null;
        if (context.TryGet(GraphContextKeys.ConditionLastResult, out bool conditionValue))
        {
            conditionResult = conditionValue;
        }

        string buildingId = null;
        context.TryGet(GraphContextKeys.BuildingLastRequestedId, out buildingId);

        string questId = null;
        context.TryGet(GraphContextKeys.QuestLastRequestedId, out questId);

        ServerActionResult serverResult = null;
        context.TryGet(GraphContextKeys.ServerLastResult, out serverResult);

        return new GraphContextSnapshot(
            choiceIndex,
            choiceLabel,
            conditionResult,
            buildingId,
            questId,
            serverResult);
    }
}
