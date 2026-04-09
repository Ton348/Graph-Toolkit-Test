using System;
using Unity.GraphToolkit.Editor;

namespace GraphCore.BaseNodes.Editor.Utility
{
    [Serializable]
    [UseWithGraph(typeof(BaseGraphEditorGraph))]
    public sealed class LogNodeModel : Graph.Core.Editor.GraphEditorNode
    {
        public const string MESSAGE_OPTION = "Message";

        protected override string DefaultTitle => "Лог";
        protected override string DefaultDescription => "Выводит сообщение в консоль";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            context.AddOption<string>(MESSAGE_OPTION).WithDisplayName("Message");
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddInputExecutionPort(context);
            AddOutputExecutionPort(context);
        }
    }
}
