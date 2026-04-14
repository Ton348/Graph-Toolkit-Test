using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public abstract class GameGraphTrueFalseNodeModel : GameGraphEditorNode
{
	public const string TRUE_PORT = GameGraphPortNames.True;
	public const string FALSE_PORT = GameGraphPortNames.False;

	protected override void OnDefinePorts(IPortDefinitionContext context)
	{
		AddInputExecutionPort(context);
		context.AddOutputPort(TRUE_PORT).WithDisplayName(TRUE_PORT).WithConnectorUI(PortConnectorUI.Arrowhead).Build();
		context.AddOutputPort(FALSE_PORT).WithDisplayName(FALSE_PORT).WithConnectorUI(PortConnectorUI.Arrowhead).Build();
	}
}
