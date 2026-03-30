using System;

[Serializable]
public class RequestBuyBuildingNode : BusinessQuestNode
{
    public string buildingId;
    public string successNodeId;
    public string failNodeId;
}
