using System;

[Serializable]
public class RequestBuyBuildingNode : BusinessQuestNode
{
    public string buildingId;
    public QuestActionType questAction;
    public string questId;
    public string successNodeId;
    public string failNodeId;
}
