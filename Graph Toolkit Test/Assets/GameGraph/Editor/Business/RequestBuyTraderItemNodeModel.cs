using System;
using Graph.Core.Editor;
using Unity.GraphToolkit.Editor;

namespace GameGraph.Editor.Business
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class RequestBuyTraderItemNodeModel : GameGraphSuccessFailNodeModel
	{
		public const string ItemIdOption = "ItemId";

		protected override string defaultTitle => "Купить предмет у торговца";
		protected override string defaultDescription => "Запрашивает покупку предмета у торговца.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(ItemIdOption).WithDisplayName("ItemId");
		}
	}
}
