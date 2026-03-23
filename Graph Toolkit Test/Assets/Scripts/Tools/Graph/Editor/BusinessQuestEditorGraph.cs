using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;

[Serializable]
[Graph(AssetExtension)]
public class BusinessQuestEditorGraph : Graph
{
    internal const string AssetExtension = "bqg";

    [MenuItem("Assets/Create/Business Quest Graph")]
    static void CreateAssetFile()
    {
        GraphDatabase.PromptInProjectBrowserToCreateNewAsset<BusinessQuestEditorGraph>("Business Quest Graph");
    }
}
