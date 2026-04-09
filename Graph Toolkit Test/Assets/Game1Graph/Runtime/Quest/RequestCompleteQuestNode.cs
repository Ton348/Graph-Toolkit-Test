using System;

[Serializable]
public class RequestCompleteQuestNode : BaseGraphNode
{
    public string questId;
    public string successNodeId;
    public string failNodeId;
}
