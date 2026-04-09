using System;
using Unity.GraphToolkit.Editor;

namespace GraphCore.BaseNodes.Editor.World
{
    [Serializable]
    [UseWithGraph(typeof(BaseGraphEditorGraph))]
    public sealed class MapMarkerNodeModel : Graph.Core.Editor.GraphEditorNode
    {
        public const string MARKER_ID_OPTION = "MarkerId";
        public const string TARGET_OPTION = "Target";

        protected override string DefaultTitle => "Метка на карте";
        protected override string DefaultDescription => "Добавляет маркер на карту";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            context.AddOption<string>(MARKER_ID_OPTION).WithDisplayName("MarkerId");
            context.AddOption<string>(TARGET_OPTION).WithDisplayName("Target");
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddInputExecutionPort(context);
            AddOutputExecutionPort(context);
        }
    }
}
