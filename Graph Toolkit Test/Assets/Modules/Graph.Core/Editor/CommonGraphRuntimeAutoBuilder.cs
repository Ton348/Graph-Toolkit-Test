using System.Collections.Generic;
using GraphCore.Runtime;
using UnityEditor;

namespace GraphCore.Editor
{
	public sealed class CommonGraphRuntimeAutoBuilder : AssetPostprocessor
	{
		private const string AutoBuildPreferenceKey = "GraphCore.AutoBuildRuntimeGraphs";
		private const string AutoBuildMenuPath = "Assets/GraphCore/Auto Build Runtime Graphs";

		private static readonly HashSet<string> s_pendingBaseGraphPaths = new HashSet<string>();
		private static readonly HashSet<string> s_pendingFirstSavePaths = new HashSet<string>();
		private static bool s_rebuildScheduled;
		private static bool s_isProcessing;
		private static CommonGraphRuntimeExporter.GraphCompiler s_graphCompiler;

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

			if (s_pendingBaseGraphPaths.Count == 0 || s_rebuildScheduled)
			{
				return;
			}

			s_rebuildScheduled = true;
			EditorApplication.delayCall += ProcessPendingBuilds;
		}

		[MenuItem(AutoBuildMenuPath, true)]
		private static bool ValidateAutoBuildMenu()
		{
			Menu.SetChecked(AutoBuildMenuPath, IsAutoBuildEnabled());
			return true;
		}

		[MenuItem(AutoBuildMenuPath)]
		private static void ToggleAutoBuild()
		{
			bool nextValue = !IsAutoBuildEnabled();
			EditorPrefs.SetBool(AutoBuildPreferenceKey, nextValue);
			Menu.SetChecked(AutoBuildMenuPath, nextValue);
		}

		private static bool IsAutoBuildEnabled()
		{
			return EditorPrefs.GetBool(AutoBuildPreferenceKey, true);
		}

		public static void SetGraphCompiler(CommonGraphRuntimeExporter.GraphCompiler graphCompiler)
		{
			s_graphCompiler = graphCompiler;
		}

		public static void ClearGraphCompiler()
		{
			s_graphCompiler = null;
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
				// Do not load GraphToolkit graph here: importer callback may run while asset is still being created.
				// Loading at this stage can trigger duplicate-path warnings in Unity/GraphToolkit.
				if (CommonGraphRuntimeExporter.IsBaseGraphAssetPath(path))
				{
					s_pendingBaseGraphPaths.Add(path);
				}
			}
		}

		private static void ProcessPendingBuilds()
		{
			EditorApplication.delayCall -= ProcessPendingBuilds;
			s_rebuildScheduled = false;

			if (!IsAutoBuildEnabled() || s_pendingBaseGraphPaths.Count == 0)
			{
				s_pendingBaseGraphPaths.Clear();
				return;
			}

			s_isProcessing = true;
			try
			{
				string[] paths = new string[s_pendingBaseGraphPaths.Count];
				s_pendingBaseGraphPaths.CopyTo(paths);
				s_pendingBaseGraphPaths.Clear();
				FilterPathsForSafeBuild(paths, out string[] safePaths);

				// Avoid forcing Refresh during graph asset creation flow. It can race with GraphToolkit graph registration.
				CommonGraphRuntimeExporter.BuildRuntimeGraphAssets(safePaths, saveAndRefresh: false, s_graphCompiler);
				AssetDatabase.SaveAssets();
			}
			finally
			{
				s_isProcessing = false;
			}
		}

		private static void FilterPathsForSafeBuild(string[] sourcePaths, out string[] safePaths)
		{
			List<string> result = new List<string>(sourcePaths.Length);
			for (int i = 0; i < sourcePaths.Length; i++)
			{
				string editorGraphPath = sourcePaths[i];
				if (string.IsNullOrWhiteSpace(editorGraphPath))
				{
					continue;
				}

				string runtimePath = CommonGraphRuntimeExporter.GetRuntimeAssetPathForEditorGraph(editorGraphPath);
				bool runtimeExists = AssetDatabase.LoadAssetAtPath<CommonGraph>(runtimePath) != null;
				if (!runtimeExists)
				{
					// First import right after graph creation is noisy in GraphToolkit; skip once, build on next save.
					if (!s_pendingFirstSavePaths.Contains(editorGraphPath))
					{
						s_pendingFirstSavePaths.Add(editorGraphPath);
						continue;
					}

					s_pendingFirstSavePaths.Remove(editorGraphPath);
				}

				result.Add(editorGraphPath);
			}

			safePaths = result.ToArray();
		}
	}
}
