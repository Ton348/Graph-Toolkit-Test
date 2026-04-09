using System;

[Serializable]
public class RequestOpenBusinessNode : BaseGraphNode
{
    public string lotId;
    public string successNodeId;
    public string failNodeId;
}
