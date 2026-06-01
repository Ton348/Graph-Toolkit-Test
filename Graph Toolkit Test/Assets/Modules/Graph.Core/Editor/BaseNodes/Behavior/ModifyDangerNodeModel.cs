using System;
using Unity.GraphToolkit.Editor;

namespace Graph.Core.Editor.BaseNodes.Behavior
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class ModifyDangerNodeModel : CommonGraphEditorNode
	{
		public const string ModeOption = "Mode";
		public const string ValueOption = "Value";

		protected override string defaultTitle => "Изменить опасность";
		protected override string defaultDescription =>
			"Mode:\n" +
			"EnableSpreadDanger — если Value > 0, отправляет danger event от NPC.\n" +
			"SetDangerRadius — изменяет радиус опасности NPC на Value.\n" +
			"SetDangerTimer — изменяет таймер опасности NPC на Value.\n" +
			"SetThreatScore — изменяет threat score NPC на Value.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<Graph.Core.Runtime.Nodes.Behavior.DangerModifyMode>(ModeOption)
				.WithDisplayName("Mode")
				.WithDefaultValue(Graph.Core.Runtime.Nodes.Behavior.DangerModifyMode.SetThreatScore);
			context.AddOption<float>(ValueOption).WithDisplayName("Value").WithDefaultValue(1f);
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			AddOutputExecutionPort(context);
		}
	}
}
