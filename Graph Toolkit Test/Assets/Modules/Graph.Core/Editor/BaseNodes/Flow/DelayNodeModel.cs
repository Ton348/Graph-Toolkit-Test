using System;
using Unity.GraphToolkit.Editor;

namespace GraphCore.BaseNodes.Editor.Flow
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class DelayNodeModel : CommonGraphEditorNode
	{
		public const string DELAY_SECONDS_OPTION = "DelaySeconds";

		protected override string DefaultTitle => "Задержка";
		protected override string DefaultDescription => "Останавливает выполнение следующих нод на время";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<float>(DELAY_SECONDS_OPTION)
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
