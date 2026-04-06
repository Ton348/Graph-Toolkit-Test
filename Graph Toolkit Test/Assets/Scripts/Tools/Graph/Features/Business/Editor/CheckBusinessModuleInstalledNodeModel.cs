using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class CheckBusinessModuleInstalledNodeModel : BusinessQuestBusinessNodeModel
{
    public const string LOT_ID_OPTION = "LotId";
    public const string MODULE_ID_OPTION = "ModuleId";

    protected override string DefaultTitle => "Модуль установлен?";
    protected override string DefaultDescription => "Проверяет, установлен ли модуль у бизнеса.";

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
        context.AddOutputPort("True").WithDisplayName("True").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("False").WithDisplayName("False").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
    }
}
