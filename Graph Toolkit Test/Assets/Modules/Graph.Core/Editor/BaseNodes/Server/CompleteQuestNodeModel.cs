using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

namespace GraphCore.BaseNodes.Editor.Server
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class CompleteQuestNodeModel : CommonGraphEditorNode
	{
		public const string QuestIdOption = "QuestId";
		public const string SuccessPort = "Success";
		public const string FailPort = "Fail";

		protected override string defaultTitle => "Завершить квест";
		protected override string defaultDescription => "Завершает квест в профиле игрока";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(QuestIdOption).WithDisplayName("QuestId");
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			context.AddOutputPort(SuccessPort).WithDisplayName("Success").WithConnectorUI(PortConnectorUI.Arrowhead)
				.Build();
			context.AddOutputPort(FailPort).WithDisplayName("Fail").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
		}
	}
}