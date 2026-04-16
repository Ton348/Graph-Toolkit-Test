using System;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

using Game1.Graph.Editor.Templates;
using Graph.Core.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class RequestTradeOfferNodeModel : GameGraphSuccessFailNodeModel
{
	public const string BuildingIdOption = "BuildingId";

	protected override string defaultTitle => "Торговое предложение";
	protected override string defaultDescription => "Запрашивает торговое предложение.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(BuildingIdOption).WithDisplayName("BuildingId");
	}
}