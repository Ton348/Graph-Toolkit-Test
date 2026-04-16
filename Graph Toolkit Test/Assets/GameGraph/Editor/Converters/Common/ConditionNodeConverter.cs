using Game1.Graph.Runtime;
using Game1.Graph.Editor;

using Game1.Graph.Editor.Infrastructure.Converters;
using Game1.Graph.Runtime.Infrastructure.AutoRegistration;
[GameGraphNodeConverter]
public sealed class ConditionNodeConverter : GameGraphNodeConverterBase<ConditionNodeModel, ConditionNode>
{
	protected override bool TryConvert(ConditionNodeModel model, out ConditionNode runtimeNode)
	{
		runtimeNode = new ConditionNode
		{
			conditionType = GetOptionValue(model, ConditionNodeModel.ConditionTypeOption, ConditionType.BuildingOwned),
			buildingId = GetOptionValue<string>(model, ConditionNodeModel.BuildingIdOption),
			requiredMoney = GetOptionValue(model, ConditionNodeModel.RequiredMoneyOption, 0),
			playerStatType = GetOptionValue(model, ConditionNodeModel.PlayerStatTypeOption, PlayerStatType.Bargaining),
			requiredStatValue = GetOptionValue(model, ConditionNodeModel.RequiredStatValueOption, 0),
			questId = GetOptionValue<string>(model, ConditionNodeModel.QuestIdOption)
		};
		return true;
	}
}

