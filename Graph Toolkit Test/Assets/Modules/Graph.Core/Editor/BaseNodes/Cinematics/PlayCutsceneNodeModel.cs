using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

namespace GraphCore.BaseNodes.Editor.Cinematics
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class PlayCutsceneNodeModel : CommonGraphEditorNode
	{
		public const string CUTSCENE_REFERENCE_OPTION = "CutsceneReference";

		protected override string DefaultTitle => "Запуск катсцены";
		protected override string DefaultDescription => "Запускает катсцену через Cinemachine";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(CUTSCENE_REFERENCE_OPTION).WithDisplayName("CutsceneReference");
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			AddOutputExecutionPort(context);
		}
	}
}
