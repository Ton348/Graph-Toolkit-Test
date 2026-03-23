using System;

[Serializable]
public class AddQuestNodeModel : BusinessQuestEditorNode
{
    public const string QUEST_OPTION = "Quest";
    protected override string DefaultTitle => "Добавить квест";
    protected override string DefaultDescription => "Добавляет игроку указанный квест.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<QuestDefinition>(QUEST_OPTION).WithDisplayName("Quest");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        AddOutputExecutionPort(context);
    }
}
