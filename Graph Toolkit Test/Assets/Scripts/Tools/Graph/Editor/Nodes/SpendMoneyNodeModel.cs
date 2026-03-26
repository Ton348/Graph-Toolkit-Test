using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class SpendMoneyNodeModel : BusinessQuestEditorNode
{
    public const string AMOUNT_OPTION = "Amount";
    public const string OPERATION_OPTION = "Operation";
    protected override string DefaultTitle => "Операция с деньгами";
    protected override string DefaultDescription => "Списывает или начисляет сумму и ведет по успеху/провалу.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<MoneyOperation>(OPERATION_OPTION)
            .WithDisplayName("Операция")
            .WithDefaultValue(MoneyOperation.Spend);
        context.AddOption<int>(AMOUNT_OPTION).WithDisplayName("Amount");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        context.AddOutputPort("Success").WithDisplayName("Success").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("Fail").WithDisplayName("Fail").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
    }
}
