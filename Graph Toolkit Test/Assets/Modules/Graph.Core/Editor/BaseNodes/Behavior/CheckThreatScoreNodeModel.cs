using System;
using Unity.GraphToolkit.Editor;

namespace Graph.Core.Editor.BaseNodes.Behavior
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class CheckThreatScoreNodeModel : CommonGraphEditorNode
	{
		public const string CompareTypeOption = "CompareType";
		public const string ValueAOption = "ValueA";
		public const string ValueBOption = "ValueB";
		public const string TruePort = "True";
		public const string FalsePort = "False";

		protected override string defaultTitle => "Проверка угрозы";
		protected override string defaultDescription => "Сравнивает текущий threatScore с заданными значениями.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<Graph.Core.Runtime.Nodes.Behavior.DangerCompareType>(CompareTypeOption)
				.WithDisplayName("CompareType")
				.WithDefaultValue(Graph.Core.Runtime.Nodes.Behavior.DangerCompareType.Greater);
			context.AddOption<int>(ValueAOption).WithDisplayName("ValueA").WithDefaultValue(10);
			context.AddOption<int>(ValueBOption).WithDisplayName("ValueB").WithDefaultValue(20);
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			context.AddOutputPort(TruePort).WithDisplayName("True").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
			context.AddOutputPort(FalsePort).WithDisplayName("False").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
		}
	}
}
