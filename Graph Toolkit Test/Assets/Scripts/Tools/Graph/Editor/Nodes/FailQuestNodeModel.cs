using System;

[Serializable]
public class FailQuestNodeModel : BusinessQuestEditorNode
{
    public const string QUEST_ID_OPTION = "QuestId";
    protected override string DefaultTitle => "Провалить квест";
    protected override string DefaultDescription => "Проваливает указанный квест.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<string>(QUEST_ID_OPTION).WithDisplayName("Quest Id");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        AddOutputExecutionPort(context);
    }
}
