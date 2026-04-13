using System;
using Unity.GraphToolkit.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class TestLogNodeModel : GameGraphEditorNode
{
	public const string MESSAGE_OPTION = "Message";

	protected override string DefaultTitle => "Test Log";
	protected override string DefaultDescription => "Тестовая нода для проверки Game1.Graph pipeline.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(MESSAGE_OPTION).WithDisplayName("Message");
	}

	protected override void OnDefinePorts(IPortDefinitionContext context)
	{
		AddInputExecutionPort(context);

		context.AddOutputPort("Next")
			.WithDisplayName("Next")
			.WithConnectorUI(PortConnectorUI.Arrowhead)
			.Build();
	}
}