using System;

[Serializable]
public class StartNodeModel : BusinessQuestEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddOutputExecutionPort(context);
    }
}
