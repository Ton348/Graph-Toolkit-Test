using System;

[Serializable]
public class GiveQuestNodeModel : BusinessQuestEditorNode
{
    public const string QUEST_OPTION = "Quest";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        context.AddOption<QuestDefinition>(QUEST_OPTION)
            .WithDisplayName("Quest");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        AddOutputExecutionPort(context);
    }
}
