using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

namespace GraphCore.BaseNodes.Editor.Server
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class CheckpointNodeModel : CommonGraphEditorNode
	{
		public const string CheckpointIdOption = "CheckpointId";
		public const string ActionOption = "Action";
		public const string SuccessPort = "Success";
		public const string FailPort = "Fail";

		protected override string DefaultTitle => "Управление чекпоинтом";
		protected override string DefaultDescription => "Сохраняет или удаляет checkpoint с которого начнется старт графа в профиле игрока";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(CheckpointIdOption).WithDisplayName("CheckpointId");
			context.AddOption<GraphCore.BaseNodes.Runtime.Server.CheckpointAction>(ActionOption)
				.WithDisplayName("Action")
				.WithDefaultValue(GraphCore.BaseNodes.Runtime.Server.CheckpointAction.Save);
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			context.AddOutputPort(SuccessPort).WithDisplayName("Success").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
			context.AddOutputPort(FailPort).WithDisplayName("Fail").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
		}
	}
}
