using System;
using Graph.Core.Editor;
using Unity.GraphToolkit.Editor;

namespace GameGraph.Editor.Business
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class RequestAssignBusinessTypeNodeModel : GameGraphSuccessFailNodeModel
	{
		public const string LotIdOption = "LotId";
		public const string BusinessTypeIdOption = "BusinessTypeId";

		protected override string defaultTitle => "Назначить тип бизнеса";
		protected override string defaultDescription => "Назначает тип бизнеса для участка.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(LotIdOption).WithDisplayName("LotId");
			context.AddOption<string>(BusinessTypeIdOption).WithDisplayName("BusinessTypeId");
		}
	}
}