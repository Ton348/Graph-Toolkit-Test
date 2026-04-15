using System;
using Game1.Graph.Runtime;

using Game1.Graph.Runtime.Templates;
[Serializable]
public sealed class RequestBuyBuildingNode : GameGraphSuccessFailNode
{
    public string buildingId;
    public QuestActionType questAction;
    public string questId;
}
