using System;
using Unity.GraphToolkit.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class ConditionNodeModel : GameGraphTrueFalseNodeModel
{
	public const string CONDITION_TYPE_OPTION = "ConditionType";
	public const string BUILDING_ID_OPTION = "BuildingId";
	public const string REQUIRED_MONEY_OPTION = "RequiredMoney";
	public const string PLAYER_STAT_TYPE_OPTION = "PlayerStatType";
	public const string REQUIRED_STAT_VALUE_OPTION = "RequiredStatValue";
	public const string QUEST_ID_OPTION = "QuestId";

	protected override string DefaultTitle => "Проверка условия";
	protected override string DefaultDescription => "Проверяет условие и выбирает ветку Истина/Ложь.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<ConditionType>(CONDITION_TYPE_OPTION).WithDisplayName("ConditionType");
		context.AddOption<string>(BUILDING_ID_OPTION).WithDisplayName("BuildingId");
		context.AddOption<int>(REQUIRED_MONEY_OPTION).WithDisplayName("RequiredMoney");
		context.AddOption<PlayerStatType>(PLAYER_STAT_TYPE_OPTION).WithDisplayName("PlayerStatType");
		context.AddOption<int>(REQUIRED_STAT_VALUE_OPTION).WithDisplayName("RequiredStatValue");
		context.AddOption<string>(QUEST_ID_OPTION).WithDisplayName("QuestId");
	}

}
