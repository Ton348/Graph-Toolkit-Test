using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class RequestAssignSupplierNodeModel : BusinessQuestBusinessNodeModel
{
    public const string LOT_ID_OPTION = "LotId";
    public const string SUPPLIER_ID_OPTION = "SupplierId";

    protected override string DefaultTitle => "Назначить поставщика";
    protected override string DefaultDescription => "Отправляет запрос на назначение поставщика.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<string>(LOT_ID_OPTION)
            .WithDisplayName("Lot Id");
        context.AddOption<string>(SUPPLIER_ID_OPTION)
            .WithDisplayName("Supplier Id");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        context.AddOutputPort("Success").WithDisplayName("Success").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("Fail").WithDisplayName("Fail").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
    }
}
