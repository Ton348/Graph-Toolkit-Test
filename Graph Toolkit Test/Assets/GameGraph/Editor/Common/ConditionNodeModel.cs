using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class ConditionNodeModel : GameGraphTrueFalseNodeModel
{
	public const string ConditionTypeOption = "ConditionType";
	public const string BuildingIdOption = "BuildingId";
	public const string RequiredMoneyOption = "RequiredMoney";
	public const string PlayerStatTypeOption = "PlayerStatType";
	public const string RequiredStatValueOption = "RequiredStatValue";
	public const string QuestIdOption = "QuestId";

	protected override string DefaultTitle => "Проверка условия";
	protected override string DefaultDescription => "Проверяет условие и выбирает ветку Истина/Ложь.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<ConditionType>(ConditionTypeOption).WithDisplayName("ConditionType");
		context.AddOption<string>(BuildingIdOption).WithDisplayName("BuildingId");
		context.AddOption<int>(RequiredMoneyOption).WithDisplayName("RequiredMoney");
		context.AddOption<PlayerStatType>(PlayerStatTypeOption).WithDisplayName("PlayerStatType");
		context.AddOption<int>(RequiredStatValueOption).WithDisplayName("RequiredStatValue");
		context.AddOption<string>(QuestIdOption).WithDisplayName("QuestId");
	}

}
