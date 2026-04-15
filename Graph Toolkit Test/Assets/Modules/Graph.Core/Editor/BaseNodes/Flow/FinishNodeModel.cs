using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

namespace GraphCore.BaseNodes.Editor.Flow
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class FinishNodeModel : CommonGraphEditorNode
	{
		protected override string DefaultTitle => "Завершение графа";
		protected override string DefaultDescription => "Завершает выполнение сценария";

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
		}
	}
}
