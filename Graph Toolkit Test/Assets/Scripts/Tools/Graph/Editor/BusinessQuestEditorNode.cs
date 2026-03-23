using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public abstract class BusinessQuestEditorNode : Node
{
    public const string EXECUTION_PORT_NAME = "Next";

    protected void AddInputExecutionPort(IPortDefinitionContext context)
    {
        context.AddInputPort(EXECUTION_PORT_NAME)
            .WithDisplayName(string.Empty)
            .WithConnectorUI(PortConnectorUI.Arrowhead)
            .Build();
    }

    protected void AddOutputExecutionPort(IPortDefinitionContext context)
    {
        context.AddOutputPort(EXECUTION_PORT_NAME)
            .WithDisplayName(string.Empty)
            .WithConnectorUI(PortConnectorUI.Arrowhead)
            .Build();
    }
}
