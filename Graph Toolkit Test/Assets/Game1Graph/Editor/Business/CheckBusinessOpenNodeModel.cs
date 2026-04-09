using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class CheckBusinessOpenNodeModel : BusinessQuestBusinessNodeModel
{
    public const string LOT_ID_OPTION = "LotId";

    protected override string DefaultTitle => "Бизнес открыт?";
    protected override string DefaultDescription => "Проверяет, открыт ли бизнес.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<string>(LOT_ID_OPTION)
            .WithDisplayName("Lot Id");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        context.AddOutputPort("True").WithDisplayName("True").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("False").WithDisplayName("False").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
    }
}
