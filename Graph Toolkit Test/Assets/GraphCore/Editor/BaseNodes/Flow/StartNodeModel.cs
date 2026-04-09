using System;
using Unity.GraphToolkit.Editor;

namespace GraphCore.BaseNodes.Editor.Flow
{
    [Serializable]
    [UseWithGraph(typeof(BaseGraphEditorGraph))]
    public sealed class StartNodeModel : BaseGraphEditorNode
    {
        protected override string DefaultTitle => "Начало графа";
        protected override string DefaultDescription => "Стартовая точка выполнения сценария";

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddOutputExecutionPort(context);
        }
    }
}
