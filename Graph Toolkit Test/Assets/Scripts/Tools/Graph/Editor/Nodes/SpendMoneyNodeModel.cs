using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class SpendMoneyNodeModel : BusinessQuestEditorNode
{
    public const string AMOUNT_OPTION = "Amount";
    protected override string DefaultTitle => "Потратить деньги";
    protected override string DefaultDescription => "Пытается списать сумму и ведет по успеху/провалу.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<int>(AMOUNT_OPTION).WithDisplayName("Amount");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        context.AddOutputPort("Success").WithDisplayName("Success").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("Fail").WithDisplayName("Fail").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
    }
}
