using System;
using Unity.GraphToolkit.Editor;

namespace GraphCore.BaseNodes.Editor.Server
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class QuestStateConditionNodeModel : CommonGraphEditorNode
	{
		public const string QUEST_ID_OPTION = "QuestId";
		public const string STATE_OPTION = "State";
		public const string TRUE_PORT = "True";
		public const string FALSE_PORT = "False";

		protected override string DefaultTitle => "Проверка квеста";
		protected override string DefaultDescription => "Проверяет состояние квеста";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(QUEST_ID_OPTION).WithDisplayName("QuestId");
			context.AddOption<GraphCore.BaseNodes.Runtime.Server.QuestState>(STATE_OPTION)
				.WithDisplayName("State")
				.WithDefaultValue(GraphCore.BaseNodes.Runtime.Server.QuestState.None);
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			context.AddOutputPort(TRUE_PORT).WithDisplayName("True").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
			context.AddOutputPort(FALSE_PORT).WithDisplayName("False").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
		}
	}
}
