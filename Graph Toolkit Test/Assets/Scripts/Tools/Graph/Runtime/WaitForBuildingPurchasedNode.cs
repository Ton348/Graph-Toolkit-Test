using System;

[Serializable]
public class WaitForBuildingPurchasedNode : BusinessQuestNode
{
    public string buildingId;

    public WaitForBuildingPurchasedNode()
    {
        Title = "Ожидание покупки здания";
        Description = "Ожидает, пока игрок купит указанное здание.";
    }
}
