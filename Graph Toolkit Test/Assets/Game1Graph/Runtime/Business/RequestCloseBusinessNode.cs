using System;

[Serializable]
public class RequestCloseBusinessNode : BaseGraphNode
{
    public string lotId;
    public string successNodeId;
    public string failNodeId;
}
