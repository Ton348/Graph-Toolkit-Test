using System;

[Serializable]
public class GiveQuestNodeModel : BusinessQuestEditorNode
{
    public const string QUEST_OPTION = "Quest";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<QuestDefinition>(QUEST_OPTION)
            .WithDisplayName("Quest");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        AddOutputExecutionPort(context);
    }

    protected override string DefaultTitle => "Выдать квест";
    protected override string DefaultDescription => "Выдает игроку указанный квест.";
}
