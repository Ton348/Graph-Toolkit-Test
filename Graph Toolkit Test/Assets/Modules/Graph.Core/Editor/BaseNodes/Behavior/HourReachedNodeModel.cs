using System;
using Unity.GraphToolkit.Editor;

namespace Graph.Core.Editor.BaseNodes.Behavior
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class HourReachedNodeModel : CommonGraphEditorNode
	{
		public const string HourOption = "Hour";

		protected override string defaultTitle => "Ожидание часа";
		protected override string defaultDescription => "Ожидает наступления указанного игрового часа.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<int>(HourOption).WithDisplayName("Hour").WithDefaultValue(9);
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			AddOutputExecutionPort(context);
		}
	}
}
