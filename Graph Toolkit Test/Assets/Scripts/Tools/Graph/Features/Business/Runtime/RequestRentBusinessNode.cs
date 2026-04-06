using System;

[Serializable]
public class RequestRentBusinessNode : BusinessQuestNode
{
    public string lotId;
    public string successNodeId;
    public string failNodeId;
}
