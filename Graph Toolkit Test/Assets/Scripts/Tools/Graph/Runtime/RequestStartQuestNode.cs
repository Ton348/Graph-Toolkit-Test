using System;

[Serializable]
public class RequestStartQuestNode : BusinessQuestNode
{
    public string questId;
    public string successNodeId;
    public string failNodeId;
}
