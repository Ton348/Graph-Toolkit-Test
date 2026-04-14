using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public abstract class GameGraphSuccessFailNodeModel : GameGraphEditorNode
{
	public const string SUCCESS_PORT = GameGraphPortNames.Success;
	public const string FAIL_PORT = GameGraphPortNames.Fail;

	protected override void OnDefinePorts(IPortDefinitionContext context)
	{
		AddInputExecutionPort(context);
		context.AddOutputPort(SUCCESS_PORT).WithDisplayName(SUCCESS_PORT).WithConnectorUI(PortConnectorUI.Arrowhead).Build();
		context.AddOutputPort(FAIL_PORT).WithDisplayName(FAIL_PORT).WithConnectorUI(PortConnectorUI.Arrowhead).Build();
	}
}
