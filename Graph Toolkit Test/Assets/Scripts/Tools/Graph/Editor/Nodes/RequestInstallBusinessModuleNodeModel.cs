using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class RequestInstallBusinessModuleNodeModel : BusinessQuestBusinessNodeModel
{
    public const string LOT_ID_OPTION = "LotId";
    public const string MODULE_ID_OPTION = "ModuleId";

    protected override string DefaultTitle => "Установить модуль";
    protected override string DefaultDescription => "Отправляет запрос на установку модуля бизнеса.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<string>(LOT_ID_OPTION)
            .WithDisplayName("Lot Id");
        context.AddOption<string>(MODULE_ID_OPTION)
            .WithDisplayName("Module Id");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        context.AddOutputPort("Success").WithDisplayName("Success").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("Fail").WithDisplayName("Fail").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
    }
}
