using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

using Game1.Graph.Editor.Templates;
using GraphCore.Runtime;
[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class RequestBuyBuildingNodeModel : GameGraphSuccessFailNodeModel
{
	public const string BuildingIdOption = "BuildingId";
	public const string QuestActionOption = "QuestAction";
	public const string QuestIdOption = "QuestId";

	protected override string defaultTitle => "Купить здание";
	protected override string defaultDescription => "Запрашивает покупку здания.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(BuildingIdOption).WithDisplayName("BuildingId");
		context.AddOption<QuestActionType>(QuestActionOption).WithDisplayName("QuestAction");
		context.AddOption<string>(QuestIdOption).WithDisplayName("QuestId");
	}
}