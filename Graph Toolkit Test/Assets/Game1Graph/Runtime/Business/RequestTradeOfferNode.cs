using System;

[Serializable]
public class RequestTradeOfferNode : BusinessQuestNode
{
    public string buildingId;
    public string successNodeId;
    public string failNodeId;
}
