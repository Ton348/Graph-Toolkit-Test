using System;
using Unity.GraphToolkit.Editor;

namespace Graph.Core.Editor.BaseNodes.Behavior
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class WaitForEventNodeModel : CommonGraphEditorNode
	{
		public const string EventNameOption = "EventName";

		protected override string defaultTitle => "Ожидание события";
		protected override string defaultDescription =>
			"Ожидает возникновения указанного runtime-события перед продолжением выполнения.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(EventNameOption).WithDisplayName("EventName").WithDefaultValue(string.Empty);
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			AddOutputExecutionPort(context);
		}
	}
}
