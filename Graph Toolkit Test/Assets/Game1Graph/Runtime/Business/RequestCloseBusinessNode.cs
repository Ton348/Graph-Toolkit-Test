using System;

[Serializable]
public class RequestCloseBusinessNode : BusinessQuestNode
{
    public string lotId;
    public string successNodeId;
    public string failNodeId;
}
