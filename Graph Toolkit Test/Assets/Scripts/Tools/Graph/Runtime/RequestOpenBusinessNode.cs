using System;

[Serializable]
public class RequestOpenBusinessNode : BusinessQuestNode
{
    public string lotId;
    public string successNodeId;
    public string failNodeId;
}
