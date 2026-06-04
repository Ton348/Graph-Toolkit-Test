using System;
using Unity.GraphToolkit.Editor;

namespace Graph.Core.Editor.BaseNodes.Behavior
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class CombatActionNodeModel : CommonGraphEditorNode
	{
		public const string ActionTypeOption = "ActionType";
		public const string ValueOption = "Value";
		public const string SuccessPort = "Success";
		public const string FailPort = "Fail";

		protected override string defaultTitle => "Боевое действие";
		protected override string defaultDescription => "Выполняет боевое действие NPC через текущий combat context.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<Graph.Core.Runtime.Nodes.Behavior.CombatActionType>(ActionTypeOption)
				.WithDisplayName("ActionType")
				.WithDefaultValue(Graph.Core.Runtime.Nodes.Behavior.CombatActionType.FindThreatSource);
			context.AddOption<float>(ValueOption).WithDisplayName("Value").WithDefaultValue(10f);
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			context.AddOutputPort(SuccessPort).WithDisplayName("Success").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
			context.AddOutputPort(FailPort).WithDisplayName("Fail").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
		}
	}
}
