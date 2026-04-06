using System;
using Unity.GraphToolkit.Editor;

[UseWithGraph(typeof(Graph.Core.Editor.GraphEditorGraph))]
[Serializable]
public class RequestTradeOfferNodeModel : BusinessQuestBusinessNodeModel
{
    public const string BUILDING_ID_OPTION = "BuildingId";

    protected override string DefaultTitle => "Торг";
    protected override string DefaultDescription => "Открывает окно торговли за здание и отправляет серверный запрос после подтверждения.";

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
