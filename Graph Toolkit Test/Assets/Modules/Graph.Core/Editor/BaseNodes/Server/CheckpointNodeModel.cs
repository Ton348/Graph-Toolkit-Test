using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

namespace GraphCore.BaseNodes.Editor.Server
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class CheckpointNodeModel : CommonGraphEditorNode
	{
		public const string CHECKPOINT_ID_OPTION = "CheckpointId";
		public const string ACTION_OPTION = "Action";
		public const string SUCCESS_PORT = "Success";
		public const string FAIL_PORT = "Fail";

		protected override string DefaultTitle => "Управление чекпоинтом";
		protected override string DefaultDescription => "Сохраняет или удаляет checkpoint с которого начнется старт графа в профиле игрока";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(CHECKPOINT_ID_OPTION).WithDisplayName("CheckpointId");
			context.AddOption<GraphCore.BaseNodes.Runtime.Server.CheckpointAction>(ACTION_OPTION)
				.WithDisplayName("Action")
				.WithDefaultValue(GraphCore.BaseNodes.Runtime.Server.CheckpointAction.Save);
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			context.AddOutputPort(SUCCESS_PORT).WithDisplayName("Success").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
			context.AddOutputPort(FAIL_PORT).WithDisplayName("Fail").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
		}
	}
}
