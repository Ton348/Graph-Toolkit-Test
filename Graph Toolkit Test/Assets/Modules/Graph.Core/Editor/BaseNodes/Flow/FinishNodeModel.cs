using GraphCore.Editor;
using System;
using Unity.GraphToolkit.Editor;

namespace GraphCore.Editor.BaseNodes.Flow
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class FinishNodeModel : CommonGraphEditorNode
	{
		protected override string defaultTitle => "Завершение графа";
		protected override string defaultDescription => "Завершает выполнение сценария";

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
		}
	}
}