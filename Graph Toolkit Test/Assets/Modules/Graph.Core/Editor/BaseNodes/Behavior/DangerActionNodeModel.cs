using System;
using Unity.GraphToolkit.Editor;

namespace Graph.Core.Editor.BaseNodes.Behavior
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class DangerActionNodeModel : CommonGraphEditorNode
	{
		public const string ActionTypeOption = "ActionType";
		public const string ValueOption = "Value";
		public const string SpeedDeltaOption = "SpeedDelta";
		public const string SuccessPort = "Success";

		protected override string defaultTitle => "Действие при опасности";
		protected override string defaultDescription => "Выполняет движение или freeze действие для danger поведения NPC.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<Graph.Core.Runtime.Nodes.Behavior.DangerActionType>(ActionTypeOption)
				.WithDisplayName("ActionType")
				.WithDefaultValue(Graph.Core.Runtime.Nodes.Behavior.DangerActionType.RunFromDanger);
			context.AddOption<float>(ValueOption).WithDisplayName("Value").WithDefaultValue(30f);
			context.AddOption<float>(SpeedDeltaOption).WithDisplayName("SpeedDelta").WithDefaultValue(0f);
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			context.AddOutputPort(SuccessPort).WithDisplayName("Success").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
		}
	}
}
