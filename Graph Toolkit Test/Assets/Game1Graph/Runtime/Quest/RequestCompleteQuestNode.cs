using System;

[Serializable]
public class RequestCompleteQuestNode : BusinessQuestNode
{
    public string questId;
    public string successNodeId;
    public string failNodeId;
}
