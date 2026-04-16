using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

namespace GraphCore.BaseNodes.Editor.Cinematics
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class PlayCutsceneNodeModel : CommonGraphEditorNode
	{
		public const string CutsceneReferenceOption = "CutsceneReference";

		protected override string defaultTitle => "Запуск катсцены";
		protected override string defaultDescription => "Запускает катсцену через Cinemachine";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(CutsceneReferenceOption).WithDisplayName("CutsceneReference");
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			AddOutputExecutionPort(context);
		}
	}
}