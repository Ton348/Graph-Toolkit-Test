using System;
using Graph.Core.Editor;
using Unity.GraphToolkit.Editor;

namespace GameGraph.Editor.Business
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class CheckBusinessExistsNodeModel : GameGraphTrueFalseNodeModel
	{
		public const string LotIdOption = "LotId";

		protected override string defaultTitle => "Проверка существования бизнеса";
		protected override string defaultDescription => "Проверяет, есть ли бизнес на участке.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(LotIdOption).WithDisplayName("LotId");
		}
	}
}