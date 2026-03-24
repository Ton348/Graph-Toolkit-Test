using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class BranchByInteractionContextNodeModel : BusinessQuestEditorNode
{
    protected override string DefaultTitle => "Ветка по контексту";
    protected override string DefaultDescription => "Разделяет логику по типу взаимодействия.";

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputExecutionPort(context);
        context.AddOutputPort("Normal").WithDisplayName("Normal").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddOutputPort("Steal").WithDisplayName("Steal").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
    }
}
