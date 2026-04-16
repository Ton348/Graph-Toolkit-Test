using GraphCore.Editor;
using System;
using Unity.GraphToolkit.Editor;

namespace GraphCore.BaseNodes.Editor.Cinematics
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class PlayCutsceneNodeModel : CommonGraphEditorNode
	{
		public const string CutsceneReferenceOption = "CutsceneReference";

		protected override string DefaultTitle => "Запуск катсцены";
		protected override string DefaultDescription => "Запускает катсцену через Cinemachine";

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
