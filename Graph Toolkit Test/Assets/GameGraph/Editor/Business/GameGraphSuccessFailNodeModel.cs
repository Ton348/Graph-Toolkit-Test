using System;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

[Serializable]
public abstract class GameGraphSuccessFailNodeModel : GameGraphEditorNode
{
	public const string SuccessPort = GameGraphPortNames.Success;
	public const string FailPort = GameGraphPortNames.Fail;

	protected override void OnDefinePorts(IPortDefinitionContext context)
	{
		AddInputExecutionPort(context);
		context.AddOutputPort(SuccessPort).WithDisplayName(SuccessPort).WithConnectorUI(PortConnectorUI.Arrowhead).Build();
		context.AddOutputPort(FailPort).WithDisplayName(FailPort).WithConnectorUI(PortConnectorUI.Arrowhead).Build();
	}
}
