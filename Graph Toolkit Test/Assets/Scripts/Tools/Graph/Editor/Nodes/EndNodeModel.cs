using System;

[Serializable]
public class EndNodeModel : BusinessQuestEditorNode
{
    public const string CLEAR_CHECKPOINT_OPTION = "ClearCheckpoint";
    public const string COMPLETE_QUEST_ID_OPTION = "CompleteQuestId";

    protected override string DefaultTitle => "Конец";
    protected override string DefaultDescription => "Завершает выполнение графа.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);
        context.AddOption<bool>(CLEAR_CHECKPOINT_OPTION).WithDisplayName("Сбросить чекпоинт").WithDefaultValue(true);
        context.AddOption<string>(COMPLETE_QUEST_ID_OPTION).WithDisplayName("ID квеста (завершить)");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
    }
}
