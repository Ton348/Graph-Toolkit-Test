using System;

[Serializable]
public class WaitForBuildingUpgradedNode : BusinessQuestNode
{
    public string buildingId;

    public WaitForBuildingUpgradedNode()
    {
        Title = "Ожидание улучшения здания";
        Description = "Ожидает, пока игрок улучшит указанное здание.";
    }
}
