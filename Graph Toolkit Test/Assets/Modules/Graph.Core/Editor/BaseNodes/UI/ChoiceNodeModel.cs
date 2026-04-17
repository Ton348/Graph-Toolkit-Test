using System;
using Unity.GraphToolkit.Editor;

namespace Graph.Core.Editor.BaseNodes.UI
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class ChoiceNodeModel : CommonGraphEditorNode
	{
		public const string Option1Label = "Option1Label";
		public const string Option2Label = "Option2Label";
		public const string Option3Label = "Option3Label";
		public const string Option4Label = "Option4Label";

		public const string Option1Port = "Option1";
		public const string Option2Port = "Option2";
		public const string Option3Port = "Option3";
		public const string Option4Port = "Option4";

		protected override string defaultTitle => "Выбор игрока";
		protected override string defaultDescription => "Показывает варианты ответа игрока";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);

			context.AddOption<string>(Option1Label)
				.WithDisplayName("Option1");

			context.AddOption<string>(Option2Label)
				.WithDisplayName("Option2");

			context.AddOption<string>(Option3Label)
				.WithDisplayName("Option3");

			context.AddOption<string>(Option4Label)
				.WithDisplayName("Option4");
		}

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);

			context.AddOutputPort(Option1Port)
				.WithDisplayName("Option1")
				.WithConnectorUI(PortConnectorUI.Arrowhead)
				.Build();

			context.AddOutputPort(Option2Port)
				.WithDisplayName("Option2")
				.WithConnectorUI(PortConnectorUI.Arrowhead)
				.Build();

			context.AddOutputPort(Option3Port)
				.WithDisplayName("Option3")
				.WithConnectorUI(PortConnectorUI.Arrowhead)
				.Build();

			context.AddOutputPort(Option4Port)
				.WithDisplayName("Option4")
				.WithConnectorUI(PortConnectorUI.Arrowhead)
				.Build();
		}
	}
}