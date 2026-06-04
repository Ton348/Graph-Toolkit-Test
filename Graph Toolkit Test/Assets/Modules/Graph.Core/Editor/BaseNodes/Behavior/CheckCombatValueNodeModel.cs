using System;
using Unity.GraphToolkit.Editor;

namespace Graph.Core.Editor.BaseNodes.Behavior
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class CheckCombatValueNodeModel : CommonGraphEditorNode
	{
		public const string ValueTypeOption = "ValueType";
		public const string ComparisonTypeOption = "ComparisonType";
		public const string ValueOption = "Value";
		public const string TruePort = "True";
		public const string FalsePort = "False";

		protected override string defaultTitle => "Проверка боевого значения";
		protected override string defaultDescription => "Проверяет боевой параметр NPC и выбирает следующую ветку выполнения.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<Graph.Core.Runtime.Nodes.Behavior.CombatValueType>(ValueTypeOption)
				.WithDisplayName("ValueType")
				.WithDefaultValue(Graph.Core.Runtime.Nodes.Behavior.CombatValueType.ThreatScore);
			context.AddOption<Graph.Core.Runtime.Nodes.Behavior.CombatComparisonType>(ComparisonTypeOption)
				.WithDisplayName("ComparisonType")
				.WithDefaultValue(Graph.Core.Runtime.Nodes.Behavior.CombatComparisonType.Greater);
			context.AddOption<float>(ValueOption).WithDisplayName("Value").WithDefaultValue(10f);
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			context.AddOutputPort(TruePort).WithDisplayName("True").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
			context.AddOutputPort(FalsePort).WithDisplayName("False").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
		}
	}
}
