using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class RefreshProfileNodeModel : BusinessQuestQuestNodeModel
{
    protected override string DefaultTitle => "Обновить профиль";
    protected override string DefaultDescription => "Запрашивает актуальный профиль у сервера.";

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        context.AddOutputPort("Success").WithDisplayName("Success").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("Fail").WithDisplayName("Fail").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
    }
}
