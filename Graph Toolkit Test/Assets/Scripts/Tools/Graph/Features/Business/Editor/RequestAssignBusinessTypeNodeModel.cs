using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class RequestAssignBusinessTypeNodeModel : BusinessQuestBusinessNodeModel
{
    public const string LOT_ID_OPTION = "LotId";
    public const string BUSINESS_TYPE_ID_OPTION = "BusinessTypeId";

    protected override string DefaultTitle => "Назначить тип бизнеса";
    protected override string DefaultDescription => "Отправляет запрос на назначение типа бизнеса.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<string>(LOT_ID_OPTION)
            .WithDisplayName("Lot Id");
        context.AddOption<string>(BUSINESS_TYPE_ID_OPTION)
            .WithDisplayName("Business Type Id");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        context.AddOutputPort("Success").WithDisplayName("Success").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("Fail").WithDisplayName("Fail").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
    }
}
