using System;
using Graph.Core.Editor;
using Unity.GraphToolkit.Editor;

namespace GameGraph.Editor.Business
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class RequestRentBusinessNodeModel : GameGraphSuccessFailNodeModel
	{
		public const string LotIdOption = "LotId";

		protected override string defaultTitle => "Арендовать бизнес";
		protected override string defaultDescription => "Запрашивает аренду бизнеса.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(LotIdOption).WithDisplayName("LotId");
		}
	}
}