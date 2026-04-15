using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

namespace GraphCore.BaseNodes.Editor.UI
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class ChoiceNodeModel : CommonGraphEditorNode
	{
		public const string OPTION1_LABEL = "Option1Label";
		public const string OPTION2_LABEL = "Option2Label";
		public const string OPTION3_LABEL = "Option3Label";
		public const string OPTION4_LABEL = "Option4Label";

		public const string OPTION1_PORT = "Option1";
		public const string OPTION2_PORT = "Option2";
		public const string OPTION3_PORT = "Option3";
		public const string OPTION4_PORT = "Option4";

		protected override string DefaultTitle => "Выбор игрока";
		protected override string DefaultDescription => "Показывает варианты ответа игрока";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			context.AddOption<string>(OPTION1_LABEL).WithDisplayName("Option1");
			context.AddOption<string>(OPTION2_LABEL).WithDisplayName("Option2");
			context.AddOption<string>(OPTION3_LABEL).WithDisplayName("Option3");
			context.AddOption<string>(OPTION4_LABEL).WithDisplayName("Option4");
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
