using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class StartNodeModel : BusinessQuestCommonNodeModel
{
    protected override string DefaultTitle => "Старт";
    protected override string DefaultDescription => "Начальная точка графа.";

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddOutputExecutionPort(context);
    }
}
