using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class RequestHireBusinessWorkerNodeModel : BusinessQuestBusinessNodeModel
{
    public const string LOT_ID_OPTION = "LotId";
    public const string ROLE_ID_OPTION = "RoleId";
    public const string CONTACT_ID_OPTION = "ContactId";

    protected override string DefaultTitle => "Нанять работника";
    protected override string DefaultDescription => "Отправляет запрос на найм работника бизнеса.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<string>(LOT_ID_OPTION)
            .WithDisplayName("Lot Id");
        context.AddOption<string>(ROLE_ID_OPTION)
            .WithDisplayName("Role Id");
        context.AddOption<string>(CONTACT_ID_OPTION)
            .WithDisplayName("Contact Id");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        context.AddOutputPort("Success").WithDisplayName("Success").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("Fail").WithDisplayName("Fail").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
    }
}
