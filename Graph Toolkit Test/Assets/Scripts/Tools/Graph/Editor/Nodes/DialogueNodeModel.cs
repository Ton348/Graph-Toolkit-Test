using System;

[Serializable]
public class DialogueNodeModel : BusinessQuestEditorNode
{
    public const string TITLE_OPTION = "Title";
    public const string BODY_OPTION = "Body";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        context.AddOption<string>(TITLE_OPTION).WithDisplayName("Title");
        context.AddOption<string>(BODY_OPTION).WithDisplayName("Body");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        AddOutputExecutionPort(context);
    }
}
