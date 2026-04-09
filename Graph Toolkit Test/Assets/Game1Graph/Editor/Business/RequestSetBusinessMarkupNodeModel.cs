using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class RequestSetBusinessMarkupNodeModel : BusinessQuestBusinessNodeModel
{
    public const string LOT_ID_OPTION = "LotId";
    public const string MARKUP_OPTION = "MarkupPercent";

    protected override string DefaultTitle => "Установить наценку";
    protected override string DefaultDescription => "Отправляет запрос на установку наценки бизнеса.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<string>(LOT_ID_OPTION)
            .WithDisplayName("Lot Id");
        context.AddOption<int>(MARKUP_OPTION)
            .WithDisplayName("Markup Percent");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        context.AddOutputPort("Success").WithDisplayName("Success").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("Fail").WithDisplayName("Fail").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
    }
}
