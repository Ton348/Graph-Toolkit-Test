using System;
using System.Runtime.CompilerServices;
using UnityEditor.AssetImporters;
using UnityEditor;

namespace Graph.Core.Editor
{
    [Serializable]
    public class GraphEditorGraph : global::BusinessQuestEditorGraph
    {
    }

    [Serializable]
    public abstract class GraphEditorNode : global::BusinessQuestEditorNode
    {
    }

    internal class GraphImporter : global::BusinessQuestGraphImporter
    {
    }

    [InitializeOnLoad]
    internal static class NodeViewDecorator
    {
        static NodeViewDecorator()
        {
            RuntimeHelpers.RunClassConstructor(typeof(global::ConditionNodeViewDecorator).TypeHandle);
        }
    }
}
