using System;
using Unity.GraphToolkit.Editor;

namespace Graph.Core.Editor.BaseNodes.Behavior
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class ModifyNeedNodeModel : CommonGraphEditorNode
	{
		public const string NeedTypeOption = "NeedType";
		public const string AmountOption = "Amount";
		public const string IncreaseOption = "Increase";

		protected override string defaultTitle => "Изменить потребность";
		protected override string defaultDescription => "Увеличивает или уменьшает значение выбранной потребности NPC.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<Graph.Core.Runtime.Nodes.Behavior.BehaviorNeedType>(NeedTypeOption)
				.WithDisplayName("NeedType")
				.WithDefaultValue(Graph.Core.Runtime.Nodes.Behavior.BehaviorNeedType.Hunger);
			context.AddOption<float>(AmountOption).WithDisplayName("Amount").WithDefaultValue(20f);
			context.AddOption<bool>(IncreaseOption).WithDisplayName("Increase").WithDefaultValue(false);
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			AddOutputExecutionPort(context);
		}
	}
}
