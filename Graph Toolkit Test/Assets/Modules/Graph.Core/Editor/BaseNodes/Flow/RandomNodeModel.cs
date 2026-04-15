using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

namespace GraphCore.BaseNodes.Editor.Flow
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class RandomNodeModel : CommonGraphEditorNode
	{
		public const string WEIGHT1_OPTION = "Weight1";
		public const string WEIGHT2_OPTION = "Weight2";
		public const string WEIGHT3_OPTION = "Weight3";
		public const string WEIGHT4_OPTION = "Weight4";

		public const string OPTION1_PORT = "Option1";
		public const string OPTION2_PORT = "Option2";
		public const string OPTION3_PORT = "Option3";
		public const string OPTION4_PORT = "Option4";

		protected override string DefaultTitle => "Случайный выбор";
		protected override string DefaultDescription => "Выбирает случайную ветку";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<float>(WEIGHT1_OPTION).WithDisplayName("Weight1").WithDefaultValue(1f);
			context.AddOption<float>(WEIGHT2_OPTION).WithDisplayName("Weight2").WithDefaultValue(1f);
			context.AddOption<float>(WEIGHT3_OPTION).WithDisplayName("Weight3").WithDefaultValue(1f);
			context.AddOption<float>(WEIGHT4_OPTION).WithDisplayName("Weight4").WithDefaultValue(1f);
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			context.AddOutputPort(OPTION1_PORT).WithDisplayName("Option1").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
			context.AddOutputPort(OPTION2_PORT).WithDisplayName("Option2").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
			context.AddOutputPort(OPTION3_PORT).WithDisplayName("Option3").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
			context.AddOutputPort(OPTION4_PORT).WithDisplayName("Option4").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
		}
	}
}
