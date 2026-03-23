using System;

[Serializable]
public class StartNodeModel : BusinessQuestEditorNode
{
    protected override string DefaultTitle => "Старт";
    protected override string DefaultDescription => "Начальная точка графа.";

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddOutputExecutionPort(context);
    }
}
