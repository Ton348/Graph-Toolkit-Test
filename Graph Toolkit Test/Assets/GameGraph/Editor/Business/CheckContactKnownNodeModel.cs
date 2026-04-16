using System;
using Graph.Core.Editor;
using Unity.GraphToolkit.Editor;

namespace GameGraph.Editor.Business
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class CheckContactKnownNodeModel : GameGraphTrueFalseNodeModel
	{
		public const string ContactIdOption = "ContactId";

		protected override string defaultTitle => "Проверка знакомства с контактом";
		protected override string defaultDescription => "Проверяет, знаком ли игрок с контактом.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(ContactIdOption).WithDisplayName("ContactId");
		}
	}
}