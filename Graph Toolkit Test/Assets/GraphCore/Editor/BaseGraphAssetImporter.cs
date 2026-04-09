using System;
using System.Collections.Generic;
using Graph.Core.Editor;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, BaseGraphEditorGraph.AssetExtension)]
internal class BaseGraphAssetImporter : ScriptedImporter
{
    private const string RuntimeGraphAssetName = "RuntimeGraph";

    public override void OnImportAsset(AssetImportContext ctx)
    {
        if (ctx == null)
        {
            throw new ArgumentNullException(nameof(ctx));
        }

        BaseGraph runtimeGraph = BuildRuntimeGraph(ctx.assetPath);
        ctx.AddObjectToAsset(RuntimeGraphAssetName, runtimeGraph);
        ctx.SetMainObject(runtimeGraph);
    }

    private static BaseGraph BuildRuntimeGraph(string assetPath)
    {
        BaseGraphEditorGraph graph = GraphDatabase.LoadGraphForImporter<BaseGraphEditorGraph>(assetPath);
        BaseGraph runtimeGraph = graph != null ? BaseGraphImporter.BuildBaseGraph(graph) : null;
        return runtimeGraph ?? CreateEmptyRuntimeGraph();
    }

    private static BaseGraph CreateEmptyRuntimeGraph()
    {
        BaseGraph runtimeGraph = ScriptableObject.CreateInstance<BaseGraph>();
        runtimeGraph.startNodeId = null;
        runtimeGraph.nodes = new List<BusinessQuestNode>();
        return runtimeGraph;
    }
}
