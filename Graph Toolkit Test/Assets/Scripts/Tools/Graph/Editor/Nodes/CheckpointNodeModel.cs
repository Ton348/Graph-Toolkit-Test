using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class CheckpointNodeModel : BusinessQuestEditorNode
{
    public const string CHECKPOINT_ID_OPTION = "CheckpointId";

    protected override string DefaultTitle => "Checkpoint";
    protected override string DefaultDescription => "Сохраняет точку продолжения графа.";

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        base.OnDefineOptions(context);
        context.AddOption<string>(CHECKPOINT_ID_OPTION).WithDisplayName("Checkpoint Id");
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        AddOutputExecutionPort(context);
    }
}
