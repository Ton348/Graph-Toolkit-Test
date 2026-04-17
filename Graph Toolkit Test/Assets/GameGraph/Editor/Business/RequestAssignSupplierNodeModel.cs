using System;
using Graph.Core.Editor;
using Unity.GraphToolkit.Editor;

namespace GameGraph.Editor.Business
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class RequestAssignSupplierNodeModel : GameGraphSuccessFailNodeModel
	{
		public const string LotIdOption = "LotId";
		public const string SupplierIdOption = "SupplierId";

		protected override string defaultTitle => "Назначить поставщика";
		protected override string defaultDescription => "Назначает поставщика бизнесу.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(LotIdOption).WithDisplayName("LotId");
			context.AddOption<string>(SupplierIdOption).WithDisplayName("SupplierId");
		}
	}
}