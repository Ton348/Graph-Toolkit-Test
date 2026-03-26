using System;

[Serializable]
public class SpendMoneyNode : BusinessQuestNode
{
    public int amount;
    public MoneyOperation operation = MoneyOperation.Spend;
    public string successNodeId;
    public string failNodeId;
}
