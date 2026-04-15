using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class RequestTradeOfferNodeModel : GameGraphSuccessFailNodeModel
{
	public const string BUILDING_ID_OPTION = "BuildingId";

	protected override string DefaultTitle => "Торговое предложение";
	protected override string DefaultDescription => "Запрашивает торговое предложение.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(BUILDING_ID_OPTION).WithDisplayName("BuildingId");
	}
}
