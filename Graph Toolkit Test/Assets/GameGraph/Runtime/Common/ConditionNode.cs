using System;
using Game1.Graph.Runtime;

[Serializable]
public sealed class ConditionNode : GameGraphTrueFalseNode
{
	public ConditionType conditionType;
	public string buildingId;
	public int requiredMoney;
	public PlayerStatType playerStatType;
	public int requiredStatValue;
	public string questId;

	public ConditionNode()
	{
		Title = "Проверка условия";
		Description = "Проверяет условие и идет по ветке True/False.";
	}
}
