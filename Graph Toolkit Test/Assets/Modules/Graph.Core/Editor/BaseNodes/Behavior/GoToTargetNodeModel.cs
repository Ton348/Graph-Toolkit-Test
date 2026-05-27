using System;
using Unity.GraphToolkit.Editor;

namespace Graph.Core.Editor.BaseNodes.Behavior
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class GoToTargetNodeModel : CommonGraphEditorNode
	{
		public const string TargetTypeOption = "TargetType";

		protected override string defaultTitle => "Перейти к цели";
		protected override string defaultDescription =>
			"Отправляет NPC к целевой точке, соответствующей выбранному типу цели.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<Graph.Core.Runtime.Nodes.Behavior.BehaviorTargetType>(TargetTypeOption)
				.WithDisplayName("TargetType")
				.WithDefaultValue(Graph.Core.Runtime.Nodes.Behavior.BehaviorTargetType.Work);
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			AddOutputExecutionPort(context);
		}
	}
}
