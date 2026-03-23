using System;

[Serializable]
public class EndNodeModel : BusinessQuestEditorNode
{
    protected override string DefaultTitle => "Конец";
    protected override string DefaultDescription => "Завершает выполнение графа.";

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
    }
}
