using System;
using Unity.GraphToolkit.Editor;

namespace Graph.Core.Editor.BaseNodes.Flow
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