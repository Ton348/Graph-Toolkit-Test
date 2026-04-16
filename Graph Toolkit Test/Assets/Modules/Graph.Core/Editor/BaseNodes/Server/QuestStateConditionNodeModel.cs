using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

namespace GraphCore.BaseNodes.Editor.Server
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class QuestStateConditionNodeModel : CommonGraphEditorNode
	{
		public const string QuestIdOption = "QuestId";
		public const string StateOption = "State";
		public const string TruePort = "True";
		public const string FalsePort = "False";

		protected override string DefaultTitle => "Проверка квеста";
		protected override string DefaultDescription => "Проверяет состояние квеста";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(QuestIdOption).WithDisplayName("QuestId");
			context.AddOption<GraphCore.Runtime.Nodes.Server.QuestState>(StateOption)
				.WithDisplayName("State")
				.WithDefaultValue(GraphCore.Runtime.Nodes.Server.QuestState.None);
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			context.AddOutputPort(TruePort)
				.WithDisplayName("True")
				.WithConnectorUI(PortConnectorUI.Arrowhead)
				.Build();

			context.AddOutputPort(FalsePort)
				.WithDisplayName("False")
				.WithConnectorUI(PortConnectorUI.Arrowhead)
				.Build();
		}
	}
}
