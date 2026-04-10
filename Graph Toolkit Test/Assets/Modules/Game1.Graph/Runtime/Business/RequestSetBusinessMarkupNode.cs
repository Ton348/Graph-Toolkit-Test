using System;

[Serializable]
public class RequestSetBusinessMarkupNode : BaseGraphNode
{
    public string lotId;
    public int markupPercent;
    public string successNodeId;
    public string failNodeId;
}
