using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class RequestBuyBuildingNodeModel : BusinessQuestEditorNode
{
    public const string BUILDING_ID_OPTION = "BuildingId";

    protected override string DefaultTitle => "Запрос покупки здания";
    protected override string DefaultDescription => "Отправляет запрос на покупку здания через IGameServer.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<string>(BUILDING_ID_OPTION)
            .WithDisplayName("Building Id");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        context.AddOutputPort("Success").WithDisplayName("Success").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("Fail").WithDisplayName("Fail").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
    }
}
