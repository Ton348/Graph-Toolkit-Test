using System;

[Serializable]
public class EndNodeModel : BusinessQuestEditorNode
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
    }
}
