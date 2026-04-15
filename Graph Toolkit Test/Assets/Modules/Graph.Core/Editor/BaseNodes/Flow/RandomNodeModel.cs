using GraphCore.Editor;
using System;
using Unity.GraphToolkit.Editor;

namespace GraphCore.Editor.BaseNodes.Flow
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class RandomNodeModel : CommonGraphEditorNode
	{
		public const string Weight1Option = "Weight1";
		public const string Weight2Option = "Weight2";
		public const string Weight3Option = "Weight3";
		public const string Weight4Option = "Weight4";

		public const string Option1Port = "Option1";
		public const string Option2Port = "Option2";
		public const string Option3Port = "Option3";
		public const string Option4Port = "Option4";

		protected override string DefaultTitle => "Случайный выбор";
		protected override string DefaultDescription => "Выбирает случайную ветку";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<float>(Weight1Option).WithDisplayName("Weight1").WithDefaultValue(1f);
			context.AddOption<float>(Weight2Option).WithDisplayName("Weight2").WithDefaultValue(1f);
			context.AddOption<float>(Weight3Option).WithDisplayName("Weight3").WithDefaultValue(1f);
			context.AddOption<float>(Weight4Option).WithDisplayName("Weight4").WithDefaultValue(1f);
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			context.AddOutputPort(Option1Port).WithDisplayName("Option1").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
			context.AddOutputPort(Option2Port).WithDisplayName("Option2").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
			context.AddOutputPort(Option3Port).WithDisplayName("Option3").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
			context.AddOutputPort(Option4Port).WithDisplayName("Option4").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
		}
	}
}
