using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class RequestBuyBuildingNodeModel : GameGraphSuccessFailNodeModel
{
	public const string BUILDING_ID_OPTION = "BuildingId";
	public const string QUEST_ACTION_OPTION = "QuestAction";
	public const string QUEST_ID_OPTION = "QuestId";

	protected override string DefaultTitle => "Купить здание";
	protected override string DefaultDescription => "Запрашивает покупку здания.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(BUILDING_ID_OPTION).WithDisplayName("BuildingId");
		context.AddOption<QuestActionType>(QUEST_ACTION_OPTION).WithDisplayName("QuestAction");
		context.AddOption<string>(QUEST_ID_OPTION).WithDisplayName("QuestId");
	}
}
