using GraphCore.Editor;
using System;
using Unity.GraphToolkit.Editor;

namespace GraphCore.Editor.BaseNodes.Server
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class StartQuestNodeModel : CommonGraphEditorNode
	{
		public const string QuestIdOption = "QuestId";
		public const string SuccessPort = "Success";
		public const string FailPort = "Fail";

		protected override string DefaultTitle => "Активировать квест";
		protected override string DefaultDescription => "Активирует квест в профиле игрока";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(QuestIdOption).WithDisplayName("QuestId");
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);

			context.AddOutputPort(SuccessPort)
				.WithDisplayName("Success")
				.WithConnectorUI(PortConnectorUI.Arrowhead)
				.Build();

			context.AddOutputPort(FailPort)
				.WithDisplayName("Fail")
				.WithConnectorUI(PortConnectorUI.Arrowhead)
				.Build();
		}
	}
}
