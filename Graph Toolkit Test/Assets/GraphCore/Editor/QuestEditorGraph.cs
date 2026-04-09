using System;
using Unity.GraphToolkit.Editor;

[Serializable]
[Unity.GraphToolkit.Editor.Graph(AssetExtension)]
public class QuestEditorGraph : Unity.GraphToolkit.Editor.Graph
{
    // Legacy graph type for existing .bqg assets. New graphs must use BaseGraphEditorGraph (.basegraph).
    internal const string AssetExtension = "bqg";
}
