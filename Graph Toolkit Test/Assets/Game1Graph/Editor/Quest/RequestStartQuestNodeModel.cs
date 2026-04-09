using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class RequestStartQuestNodeModel : BusinessQuestQuestNodeModel
{
    public const string QUEST_ID_OPTION = "QuestId";

    protected override string DefaultTitle => "Запрос старта квеста";
    protected override string DefaultDescription => "Отправляет запрос на старт квеста через IGameServer.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

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
