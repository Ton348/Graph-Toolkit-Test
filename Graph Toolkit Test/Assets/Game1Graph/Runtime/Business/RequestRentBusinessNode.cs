using System;

[Serializable]
public class RequestRentBusinessNode : BaseGraphNode
{
    public string lotId;
    public string successNodeId;
    public string failNodeId;
}
