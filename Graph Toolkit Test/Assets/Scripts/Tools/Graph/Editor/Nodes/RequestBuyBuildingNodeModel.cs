using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class RequestBuyBuildingNodeModel : BusinessQuestEditorNode
{
    public const string BUILDING_ID_OPTION = "BuildingId";
    public const string QUEST_ACTION_OPTION = "QuestAction";
    public const string QUEST_ID_OPTION = "QuestId";

    protected override string DefaultTitle => "Запрос покупки здания";
    protected override string DefaultDescription => "Отправляет запрос на покупку здания через IGameServer.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<string>(BUILDING_ID_OPTION)
            .WithDisplayName("Building Id");
        context.AddOption<QuestActionType>(QUEST_ACTION_OPTION)
            .WithDisplayName("Quest Action")
            .WithDefaultValue(QuestActionType.None);
        context.AddOption<string>(QUEST_ID_OPTION)
            .WithDisplayName("Quest Id");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        context.AddOutputPort("Success").WithDisplayName("Success").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("Fail").WithDisplayName("Fail").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
    }
}
