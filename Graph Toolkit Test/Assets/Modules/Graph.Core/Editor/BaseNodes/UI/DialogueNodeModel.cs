using GraphCore.Editor;
using System;
using Unity.GraphToolkit.Editor;

namespace GraphCore.BaseNodes.Editor.UI
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class DialogueNodeModel : CommonGraphEditorNode
	{
		public const string DialogueTitleOption = "Title";
		public const string DialogueBodyOption = "Body";

		protected override string defaultTitle => "Диалог NPC";
		protected override string defaultDescription => "Показывает диалог NPC";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(DialogueTitleOption)
				.WithDisplayName("Title");

			context.AddOption<string>(DialogueBodyOption)
				.WithDisplayName("Body");
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			AddOutputExecutionPort(context);
		}
	}
}