using System;
using Graph.Core.Editor;
using Unity.GraphToolkit.Editor;

namespace GameGraph.Editor.Business
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class CheckBusinessOpenNodeModel : GameGraphTrueFalseNodeModel
	{
		public const string LotIdOption = "LotId";

		protected override string defaultTitle => "Проверка открыт ли бизнес";
		protected override string defaultDescription => "Проверяет, открыт ли бизнес.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(LotIdOption).WithDisplayName("LotId");
		}
	}
}