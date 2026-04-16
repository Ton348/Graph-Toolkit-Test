using GraphCore.Editor;
using System;
using Unity.GraphToolkit.Editor;

namespace GraphCore.BaseNodes.Editor.Utility
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class LogNodeModel : CommonGraphEditorNode
	{
		public const string MessageOption = "Message";

		protected override string defaultTitle => "Лог";
		protected override string defaultDescription => "Выводит сообщение в консоль";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);

			context.AddOption<string>(MessageOption)
				.WithDisplayName("Message");
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			AddOutputExecutionPort(context);
		}
	}
}
