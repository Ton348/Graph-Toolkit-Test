using System;
using Unity.GraphToolkit.Editor;

namespace GraphCore.BaseNodes.Editor.Flow
{
    [Serializable]
    [UseWithGraph(typeof(BaseGraphEditorGraph))]
    public sealed class FinishNodeModel : BaseGraphEditorNode
    {
        protected override string DefaultTitle => "Завершение графа";
        protected override string DefaultDescription => "Завершает выполнение сценария";

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddInputExecutionPort(context);
        }
    }
}
