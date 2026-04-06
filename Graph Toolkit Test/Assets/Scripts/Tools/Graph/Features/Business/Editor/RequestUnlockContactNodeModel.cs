using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class RequestUnlockContactNodeModel : BusinessQuestBusinessNodeModel
{
    public const string CONTACT_ID_OPTION = "ContactId";

    protected override string DefaultTitle => "Открыть контакт";
    protected override string DefaultDescription => "Отправляет запрос на открытие контакта.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

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
