using System.Collections.Generic;
using UnityEditor;

public sealed class BaseGraphRuntimeAutoBuilder : AssetPostprocessor
{
    private const string AutoBuildPreferenceKey = "GraphCore.AutoBuildRuntimeGraphs";

    private static readonly HashSet<string> PendingBaseGraphPaths = new HashSet<string>();
    private static bool s_rebuildScheduled;
    private static bool s_isProcessing;

    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (!IsAutoBuildEnabled() || s_isProcessing)
        {
            return;
        }

        EnqueueBaseGraphPaths(importedAssets);
        EnqueueBaseGraphPaths(movedAssets);

        if (PendingBaseGraphPaths.Count == 0 || s_rebuildScheduled)
        {
            return;
        }

        s_rebuildScheduled = true;
        EditorApplication.delayCall += ProcessPendingBuilds;
    }

    [MenuItem("Assets/GraphCore/Auto Build Runtime Graphs", true)]
    private static bool ValidateAutoBuildMenu()
    {
        Menu.SetChecked("Assets/GraphCore/Auto Build Runtime Graphs", IsAutoBuildEnabled());
        return true;
    }

    [MenuItem("Assets/GraphCore/Auto Build Runtime Graphs")]
    private static void ToggleAutoBuild()
    {
        bool nextValue = !IsAutoBuildEnabled();
        EditorPrefs.SetBool(AutoBuildPreferenceKey, nextValue);
        Menu.SetChecked("Assets/GraphCore/Auto Build Runtime Graphs", nextValue);
    }

    private static bool IsAutoBuildEnabled()
    {
        return EditorPrefs.GetBool(AutoBuildPreferenceKey, true);
    }

    private static void EnqueueBaseGraphPaths(string[] assetPaths)
    {
        if (assetPaths == null || assetPaths.Length == 0)
        {
            return;
        }

        for (int i = 0; i < assetPaths.Length; i++)
        {
            string path = assetPaths[i];
            if (BaseGraphRuntimeExporter.IsValidBaseGraphEditorAsset(path))
            {
                PendingBaseGraphPaths.Add(path);
            }
        }
    }

    private static void ProcessPendingBuilds()
    {
        EditorApplication.delayCall -= ProcessPendingBuilds;
        s_rebuildScheduled = false;

        if (!IsAutoBuildEnabled() || PendingBaseGraphPaths.Count == 0)
        {
            PendingBaseGraphPaths.Clear();
            return;
        }

        s_isProcessing = true;
        try
        {
            string[] paths = new string[PendingBaseGraphPaths.Count];
            PendingBaseGraphPaths.CopyTo(paths);
            PendingBaseGraphPaths.Clear();

            int builtCount = BaseGraphRuntimeExporter.BuildRuntimeGraphAssets(paths, saveAndRefresh: true);
            if (builtCount > 0)
            {
                UnityEngine.Debug.Log($"[BaseGraphRuntimeAutoBuilder] Auto-built {builtCount} runtime graph assets.");
            }
        }
        finally
        {
            s_isProcessing = false;
        }
    }
}
