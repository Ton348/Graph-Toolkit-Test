using System;

[Serializable]
public class ConditionNode : BusinessQuestNode
{
    public ConditionType conditionType;
    public string buildingId;
    public int requiredMoney;
    public PlayerStatType playerStatType;
    public int requiredStatValue;
    public string questId;
    public string trueNodeId;
    public string falseNodeId;

    public ConditionNode()
    {
        Title = "Проверка условия";
        Description = "Проверяет условие и идет по ветке True/False.";
    }
}
