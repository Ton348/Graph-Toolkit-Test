using System;
using System.Collections.Generic;
using System.IO;
using Graph.Core.Editor;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

public static class BaseGraphRuntimeExporter
{
    private const string BuildMenuPath = "Assets/GraphCore/Build Runtime Graph";
    private const string RuntimeAssetSuffix = ".runtime.asset";

    [MenuItem(BuildMenuPath, true)]
    private static bool ValidateBuildSelectedRuntimeGraphs()
    {
        string[] selectedPaths = GetSelectedBaseGraphPaths();
        return selectedPaths.Length > 0;
    }

    [MenuItem(BuildMenuPath)]
    private static void BuildSelectedRuntimeGraphs()
    {
        string[] selectedPaths = GetSelectedBaseGraphPaths();
        if (selectedPaths.Length == 0)
        {
            Debug.LogWarning("[BaseGraphRuntimeExporter] No .basegraph assets selected.");
            return;
        }

        int builtCount = 0;
        for (int i = 0; i < selectedPaths.Length; i++)
        {
            if (BuildRuntimeGraphAsset(selectedPaths[i]))
            {
                builtCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[BaseGraphRuntimeExporter] Built {builtCount}/{selectedPaths.Length} runtime graph assets.");
    }

    public static bool BuildRuntimeGraphAsset(string editorGraphPath)
    {
        if (string.IsNullOrWhiteSpace(editorGraphPath))
        {
            Debug.LogError("[BaseGraphRuntimeExporter] Editor graph path is empty.");
            return false;
        }

        try
        {
            BaseGraphEditorGraph editorGraph = GraphDatabase.LoadGraph<BaseGraphEditorGraph>(editorGraphPath);
            if (editorGraph == null)
            {
                Debug.LogError($"[BaseGraphRuntimeExporter] Could not load BaseGraphEditorGraph at '{editorGraphPath}'.");
                return false;
            }

            BaseGraph compiledGraph = BaseGraphImporter.BuildBaseGraph(editorGraph);
            if (compiledGraph == null)
            {
                Debug.LogError($"[BaseGraphRuntimeExporter] Failed to compile runtime graph from '{editorGraphPath}'. Ensure graph has a StartNode.");
                return false;
            }

            string runtimeGraphPath = GetRuntimeAssetPath(editorGraphPath);
            BaseGraph existingRuntimeGraph = AssetDatabase.LoadAssetAtPath<BaseGraph>(runtimeGraphPath);

            if (existingRuntimeGraph == null)
            {
                compiledGraph.name = Path.GetFileNameWithoutExtension(runtimeGraphPath);
                AssetDatabase.CreateAsset(compiledGraph, runtimeGraphPath);
                EditorUtility.SetDirty(compiledGraph);
            }
            else
            {
                existingRuntimeGraph.startNodeId = compiledGraph.startNodeId;
                existingRuntimeGraph.nodes = new List<BaseGraphNode>(compiledGraph.nodes ?? new List<BaseGraphNode>());
                existingRuntimeGraph.InvalidateLookup();
                EditorUtility.SetDirty(existingRuntimeGraph);
                UnityEngine.Object.DestroyImmediate(compiledGraph);
            }

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            return false;
        }
    }

    private static string[] GetSelectedBaseGraphPaths()
    {
        string[] assetGuids = Selection.assetGUIDs;
        List<string> paths = new List<string>(assetGuids.Length);

        for (int i = 0; i < assetGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            if (!path.EndsWith($".{BaseGraphEditorGraph.AssetExtension}", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            paths.Add(path);
        }

        return paths.ToArray();
    }

    private static string GetRuntimeAssetPath(string editorGraphPath)
    {
        string directory = Path.GetDirectoryName(editorGraphPath) ?? string.Empty;
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(editorGraphPath);
        return Path.Combine(directory, fileNameWithoutExtension + RuntimeAssetSuffix).Replace("\\", "/");
    }
}
