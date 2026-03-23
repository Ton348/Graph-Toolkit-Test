using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class SpendMoneyNodeModel : BusinessQuestEditorNode
{
    public const string AMOUNT_OPTION = "Amount";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        context.AddOption<int>(AMOUNT_OPTION).WithDisplayName("Amount");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        context.AddOutputPort("Success").WithDisplayName("Success").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("Fail").WithDisplayName("Fail").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
    }
}
