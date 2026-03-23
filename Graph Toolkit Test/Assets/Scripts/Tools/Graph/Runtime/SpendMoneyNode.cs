using System;

[Serializable]
public class SpendMoneyNode : BusinessQuestNode
{
    public int amount;
    public string successNodeId;
    public string failNodeId;
}
