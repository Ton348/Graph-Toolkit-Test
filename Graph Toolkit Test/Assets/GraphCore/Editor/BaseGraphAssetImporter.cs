using System.Collections.Generic;
using Graph.Core.Editor;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, BaseGraphEditorGraph.AssetExtension)]
internal class BaseGraphAssetImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        BaseGraphEditorGraph graph = GraphDatabase.LoadGraphForImporter<BaseGraphEditorGraph>(ctx.assetPath);
        BaseGraph runtimeGraph = graph != null ? BaseGraphImporter.BuildBaseGraph(graph) : null;
        if (runtimeGraph == null)
        {
            runtimeGraph = ScriptableObject.CreateInstance<BaseGraph>();
            runtimeGraph.startNodeId = null;
            runtimeGraph.nodes = new List<BusinessQuestNode>();
        }

        ctx.AddObjectToAsset("RuntimeGraph", runtimeGraph);
        ctx.SetMainObject(runtimeGraph);
    }
}
