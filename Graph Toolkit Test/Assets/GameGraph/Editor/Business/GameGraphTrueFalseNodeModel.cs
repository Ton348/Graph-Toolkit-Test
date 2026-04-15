using System;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

[Serializable]
public abstract class GameGraphTrueFalseNodeModel : GameGraphEditorNode
{
	public const string TruePort = GameGraphPortNames.True;
	public const string FalsePort = GameGraphPortNames.False;

	protected override void OnDefinePorts(IPortDefinitionContext context)
	{
		AddInputExecutionPort(context);
		context.AddOutputPort(TruePort).WithDisplayName(TruePort).WithConnectorUI(PortConnectorUI.Arrowhead).Build();
		context.AddOutputPort(FalsePort).WithDisplayName(FalsePort).WithConnectorUI(PortConnectorUI.Arrowhead).Build();
	}
}
