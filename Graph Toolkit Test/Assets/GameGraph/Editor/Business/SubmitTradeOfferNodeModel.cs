using System;
using Graph.Core.Editor;
using Unity.GraphToolkit.Editor;

namespace GameGraph.Editor.Business
{
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
}