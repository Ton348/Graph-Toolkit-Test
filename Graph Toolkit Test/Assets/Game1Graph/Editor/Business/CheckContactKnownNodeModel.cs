using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class CheckContactKnownNodeModel : BusinessQuestBusinessNodeModel
{
    public const string CONTACT_ID_OPTION = "ContactId";

    protected override string DefaultTitle => "Контакт известен?";
    protected override string DefaultDescription => "Проверяет, открыт ли контакт.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<string>(CONTACT_ID_OPTION)
            .WithDisplayName("Contact Id");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        context.AddOutputPort("True").WithDisplayName("True").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("False").WithDisplayName("False").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
    }
}
