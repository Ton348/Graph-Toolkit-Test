using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;

[Serializable]
[Unity.GraphToolkit.Editor.Graph(AssetExtension)]
public class BusinessQuestEditorGraph : Unity.GraphToolkit.Editor.Graph
{
    internal const string AssetExtension = "bqg";

    [MenuItem("Assets/Create/Business Quest Graph")]
    private static void CreateAssetFile()
    {
        GraphDatabase.PromptInProjectBrowserToCreateNewAsset<BusinessQuestEditorGraph>("Business Quest Graph");
    }
}
