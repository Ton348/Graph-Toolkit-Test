using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

namespace GraphCore.BaseNodes.Editor.Flow
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class StartNodeModel : CommonGraphEditorNode
	{
		protected override string defaultTitle => "Начало графа";
		protected override string defaultDescription => "Стартовая точка выполнения сценария";

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddOutputExecutionPort(context);
		}
	}
}