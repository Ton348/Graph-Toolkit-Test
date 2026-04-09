using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;

[Serializable]
[Unity.GraphToolkit.Editor.Graph(AssetExtension)]
public class BaseGraphEditorGraph : Unity.GraphToolkit.Editor.Graph
{
    internal const string AssetExtension = "basegraph";

    [MenuItem("Assets/Create/Base Graph")]
    private static void CreateAssetFile()
    {
        GraphDatabase.PromptInProjectBrowserToCreateNewAsset<BaseGraphEditorGraph>("Base Graph");
    }
}
