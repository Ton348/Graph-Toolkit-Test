using System;
using UnityEngine;

[Serializable]
public class DialogueNodeModel : BusinessQuestEditorNode
{
    public const string TITLE_OPTION = "Title";
    public const string BODY_OPTION = "Body";
    public const string SCREENSHOT_OPTION = "Screenshot";
    protected override string DefaultTitle => "Диалог";
    protected override string DefaultDescription => "Показывает диалог с заголовком и текстом.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);

        context.AddOption<string>(TITLE_OPTION).WithDisplayName("Title");
        context.AddOption<string>(BODY_OPTION).WithDisplayName("Body");
        context.AddOption<Sprite>(SCREENSHOT_OPTION).WithDisplayName("Screenshot");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        AddOutputExecutionPort(context);
    }
}
