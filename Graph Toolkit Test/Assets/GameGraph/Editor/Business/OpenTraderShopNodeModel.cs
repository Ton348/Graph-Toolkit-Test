using System;
using Graph.Core.Editor;
using Unity.GraphToolkit.Editor;

namespace GameGraph.Editor.Business
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class OpenTraderShopNodeModel : GameGraphSuccessFailNodeModel
	{
		public const string TraderIdOption = "TraderId";

		protected override string defaultTitle => "Открыть магазин торговца";
		protected override string defaultDescription => "Показывает список товаров торговца и выполняет покупку выбранного товара.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(TraderIdOption).WithDisplayName("TraderId");
		}
	}
}
