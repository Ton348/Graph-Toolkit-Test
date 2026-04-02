using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class RequestCloseBusinessNodeModel : BusinessQuestLegacyBusinessNodeModel
{
    public const string LOT_ID_OPTION = "LotId";

    protected override string DefaultTitle => "Закрыть бизнес";
    protected override string DefaultDescription => "Отправляет запрос на закрытие бизнеса.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<string>(LOT_ID_OPTION)
            .WithDisplayName("Lot Id");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        context.AddOutputPort("Success").WithDisplayName("Success").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("Fail").WithDisplayName("Fail").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
    }
}
