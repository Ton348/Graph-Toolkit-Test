using System;

[Serializable]
public class RequestTradeOfferNode : BaseGraphNode
{
    public string buildingId;
    public string successNodeId;
    public string failNodeId;
}
