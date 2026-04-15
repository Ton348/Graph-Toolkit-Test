using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

namespace GraphCore.BaseNodes.Editor.Server
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class StartQuestNodeModel : CommonGraphEditorNode
	{
		public const string QUEST_ID_OPTION = "QuestId";
		public const string SUCCESS_PORT = "Success";
		public const string FAIL_PORT = "Fail";

		protected override string DefaultTitle => "Активировать квест";
		protected override string DefaultDescription => "Активирует квест в профиле игрока";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(QUEST_ID_OPTION).WithDisplayName("QuestId");
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			context.AddOutputPort(SUCCESS_PORT).WithDisplayName("Success").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
			context.AddOutputPort(FAIL_PORT).WithDisplayName("Fail").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
		}
	}
}
