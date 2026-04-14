using System;

[Serializable]
public sealed class RequestBuyBuildingNode : GameGraphSuccessFailNode
{
    public string buildingId;
    public QuestActionType questAction;
    public string questId;
}
