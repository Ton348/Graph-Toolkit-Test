using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class RequestTradeOfferNodeModel : GameGraphSuccessFailNodeModel
{
	public const string BuildingIdOption = "BuildingId";

	protected override string DefaultTitle => "Торговое предложение";
	protected override string DefaultDescription => "Запрашивает торговое предложение.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(BuildingIdOption).WithDisplayName("BuildingId");
	}
}
