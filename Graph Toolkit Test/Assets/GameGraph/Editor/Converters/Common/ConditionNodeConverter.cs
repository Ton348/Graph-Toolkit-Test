using Graph.Core.Editor;

[GameGraphNodeConverter]
public sealed class ConditionNodeConverter : GameGraphNodeConverterBase<ConditionNodeModel, ConditionNode>
{
	protected override bool TryConvert(ConditionNodeModel model, out ConditionNode runtimeNode)
	{
		runtimeNode = new ConditionNode
		{
			conditionType = GetOptionValue(model, ConditionNodeModel.CONDITION_TYPE_OPTION, ConditionType.BuildingOwned),
			buildingId = GetOptionValue<string>(model, ConditionNodeModel.BUILDING_ID_OPTION),
			requiredMoney = GetOptionValue(model, ConditionNodeModel.REQUIRED_MONEY_OPTION, 0),
			playerStatType = GetOptionValue(model, ConditionNodeModel.PLAYER_STAT_TYPE_OPTION, PlayerStatType.Bargaining),
			requiredStatValue = GetOptionValue(model, ConditionNodeModel.REQUIRED_STAT_VALUE_OPTION, 0),
			questId = GetOptionValue<string>(model, ConditionNodeModel.QUEST_ID_OPTION)
		};
		return true;
	}
}

