using System;

[Serializable]
public class CompleteQuestNodeModel : BusinessQuestEditorNode
{
    public const string QUEST_ID_OPTION = "QuestId";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<string>(QUEST_ID_OPTION)
            .WithDisplayName("Quest Id");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        AddOutputExecutionPort(context);
    }

    protected override string DefaultTitle => "Завершить квест";
    protected override string DefaultDescription => "Завершает указанный квест.";
}
