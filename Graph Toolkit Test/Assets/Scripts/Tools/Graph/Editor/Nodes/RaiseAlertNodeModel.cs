using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class RaiseAlertNodeModel : BusinessQuestEditorNode
{
    public const string ALERT_MESSAGE_OPTION = "AlertMessage";

    protected override string DefaultTitle => "Тревога";
    protected override string DefaultDescription => "Поднимает тревогу (пока только лог).";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);
        context.AddOption<string>(ALERT_MESSAGE_OPTION).WithDisplayName("Сообщение");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        AddOutputExecutionPort(context);
    }
}
