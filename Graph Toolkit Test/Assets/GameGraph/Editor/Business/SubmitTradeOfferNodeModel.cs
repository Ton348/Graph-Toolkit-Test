using System;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

using Game1.Graph.Editor.Templates;
using Graph.Core.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class SubmitTradeOfferNodeModel : GameGraphSuccessFailNodeModel
{
	public const string BuildingIdOption = "BuildingId";

	protected override string defaultTitle => "Trade Offer";
	protected override string defaultDescription => "Отправляет торговое предложение по зданию.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(BuildingIdOption).WithDisplayName("BuildingId");
	}
}