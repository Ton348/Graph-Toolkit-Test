using System;

[Serializable]
public class RequestStartQuestNode : BaseGraphNode
{
    public string questId;
    public string successNodeId;
    public string failNodeId;
}
