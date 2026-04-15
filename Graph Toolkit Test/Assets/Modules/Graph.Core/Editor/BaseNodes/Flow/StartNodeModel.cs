using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

namespace GraphCore.BaseNodes.Editor.Flow
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class StartNodeModel : CommonGraphEditorNode
	{
		protected override string DefaultTitle => "Начало графа";
		protected override string DefaultDescription => "Стартовая точка выполнения сценария";

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddOutputExecutionPort(context);
		}
	}
}
