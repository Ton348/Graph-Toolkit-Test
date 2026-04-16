using GraphCore.Editor;
using System;
using Unity.GraphToolkit.Editor;

namespace GraphCore.Editor.BaseNodes.Server
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class CheckpointNodeModel : CommonGraphEditorNode
	{
		public const string CheckpointIdOption = "CheckpointId";
		public const string ActionOption = "Action";
		public const string SuccessPort = "Success";
		public const string FailPort = "Fail";

		protected override string defaultTitle => "Управление чекпоинтом";
		protected override string defaultDescription => "Сохраняет или удаляет checkpoint с которого начнется старт графа в профиле игрока";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(CheckpointIdOption).WithDisplayName("CheckpointId");
			context.AddOption<GraphCore.Runtime.Nodes.Server.CheckpointAction>(ActionOption)
				.WithDisplayName("Action")
				.WithDefaultValue(GraphCore.Runtime.Nodes.Server.CheckpointAction.Save);
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			context.AddOutputPort(SuccessPort).WithDisplayName("Success").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
			context.AddOutputPort(FailPort).WithDisplayName("Fail").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
		}
	}
}