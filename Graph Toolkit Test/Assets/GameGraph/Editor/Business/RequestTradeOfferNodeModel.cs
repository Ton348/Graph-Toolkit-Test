using System;
using Graph.Core.Editor;
using Unity.GraphToolkit.Editor;

namespace GameGraph.Editor.Business
{
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
}