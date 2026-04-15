using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

namespace GraphCore.Editor.BaseNodes.Flow
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class DelayNodeModel : CommonGraphEditorNode
	{
		public const string DelaySecondsOption = "DelaySeconds";

		protected override string DefaultTitle => "Задержка";
		protected override string DefaultDescription => "Останавливает выполнение следующих нод на время";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<float>(DelaySecondsOption)
				.WithDisplayName("DelaySeconds")
				.WithDefaultValue(1f);
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			AddOutputExecutionPort(context);
		}
	}
}
