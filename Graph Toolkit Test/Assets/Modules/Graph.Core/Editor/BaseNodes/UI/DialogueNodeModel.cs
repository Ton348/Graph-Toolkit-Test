using System;
using Unity.GraphToolkit.Editor;

namespace GraphCore.BaseNodes.Editor.UI
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class DialogueNodeModel : CommonGraphEditorNode
	{
		public const string DIALOGUE_TITLE_OPTION = "Title";
		public const string DIALOGUE_BODY_OPTION = "Body";

		protected override string DefaultTitle => "Диалог NPC";
		protected override string DefaultDescription => "Показывает диалог NPC";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(DIALOGUE_TITLE_OPTION).WithDisplayName("Title");
			context.AddOption<string>(DIALOGUE_BODY_OPTION).WithDisplayName("Body");
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			AddOutputExecutionPort(context);
		}
	}
}
