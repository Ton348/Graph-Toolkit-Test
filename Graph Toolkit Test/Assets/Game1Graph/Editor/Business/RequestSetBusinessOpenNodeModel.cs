using System;
using Unity.GraphToolkit.Editor;

public enum BusinessOpenAction
{
    Open = 0,
    Close = 1
}

[Serializable]
public class RequestSetBusinessOpenNodeModel : BusinessQuestBusinessNodeModel
{
    public const string LOT_ID_OPTION = "LotId";
    public const string ACTION_OPTION = "OpenAction";

    protected override string DefaultTitle => "Открыть/закрыть бизнес";
    protected override string DefaultDescription => "Отправляет запрос на открытие или закрытие бизнеса через IGameServer.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<string>(LOT_ID_OPTION)
            .WithDisplayName("Lot Id");

        context.AddOption<BusinessOpenAction>(ACTION_OPTION)
            .WithDisplayName("Действие")
            .WithDefaultValue(BusinessOpenAction.Open);
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        context.AddOutputPort("Success").WithDisplayName("Success").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("Fail").WithDisplayName("Fail").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
    }
}
