using System;

[Serializable]
public class WaitForConditionNode : BusinessQuestNode
{
    public ConditionType conditionType;
    public string buildingId;
    public int requiredMoney;
    public PlayerStatType playerStatType;
    public int requiredStatValue;
    public string questId;

    public WaitForConditionNode()
    {
        Title = "Ожидание условия";
        Description = "Ожидает, пока условие станет истинным.";
    }
}
