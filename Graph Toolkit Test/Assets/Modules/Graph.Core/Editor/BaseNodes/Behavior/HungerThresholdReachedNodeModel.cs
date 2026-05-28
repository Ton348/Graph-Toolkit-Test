using System;
using Unity.GraphToolkit.Editor;

namespace Graph.Core.Editor.BaseNodes.Behavior
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class HungerThresholdReachedNodeModel : CommonGraphEditorNode
	{
		public const string NeedTypeOption = "NeedType";
		public const string ThresholdOption = "Threshold";
		public const string TruePort = "True";
		public const string FalsePort = "False";

		protected override string defaultTitle => "Проверка характеристики";
		protected override string defaultDescription => "Проверяет, достигла ли выбранная характеристика заданного порога.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<Graph.Core.Runtime.Nodes.Behavior.BehaviorNeedType>(NeedTypeOption).WithDisplayName("NeedType")
				.WithDefaultValue(Graph.Core.Runtime.Nodes.Behavior.BehaviorNeedType.Hunger);
			context.AddOption<float>(ThresholdOption).WithDisplayName("Threshold").WithDefaultValue(75f);
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			context.AddOutputPort(TruePort).WithDisplayName("True").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
			context.AddOutputPort(FalsePort).WithDisplayName("False").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
		}
	}
}
