using System;
using System.Collections.Generic;
using System.IO;
using Graph.Core.Runtime;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Graph.Core.Editor
{
	public static class CommonGraphRuntimeExporter
	{
		public delegate CommonGraph GraphCompiler(CommonGraphEditorGraph editorGraph);

		public delegate bool GraphValidationHook(
			CommonGraphEditorGraph editorGraph,
			CommonGraph runtimeGraph,
			string editorGraphPath);

		private const string BuildMenuPath = "Assets/GraphCore/Build Runtime Graph";
		private const string BuildAllMenuPath = "Assets/GraphCore/Build All Runtime Graphs";
		private const string RuntimeAssetSuffix = ".runtime.asset";
		private static GraphValidationHook s_graphValidationHook;

		public static void SetGraphValidationHook(GraphValidationHook validationHook)
		{
			s_graphValidationHook = validationHook;
		}

		public static void ClearGraphValidationHook()
		{
			s_graphValidationHook = null;
		}

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
				return;
			}

			BuildRuntimeGraphAssets(selectedPaths, true);
		}

		[MenuItem(BuildAllMenuPath)]
		private static void BuildAllRuntimeGraphs()
		{
			string[] allBaseGraphGuids =
				AssetDatabase.FindAssets($"t:DefaultAsset *.{CommonGraphEditorGraph.AssetExtension}");
			if (allBaseGraphGuids == null || allBaseGraphGuids.Length == 0)
			{
				return;
			}

			var paths = new List<string>(allBaseGraphGuids.Length);
			for (var i = 0; i < allBaseGraphGuids.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(allBaseGraphGuids[i]);
				if (IsBaseGraphAssetPath(path))
				{
					paths.Add(path);
				}
			}

			BuildRuntimeGraphAssets(paths, true);
		}

		public static bool BuildRuntimeGraphAsset(string editorGraphPath)
		{
			return BuildRuntimeGraphAsset(editorGraphPath, null);
		}

		public static bool BuildRuntimeGraphAsset(string editorGraphPath, GraphCompiler compiler)
		{
			if (string.IsNullOrWhiteSpace(editorGraphPath))
			{
				Debug.LogError("[CommonGraphRuntimeExporter] Editor graph path is empty.");
				return false;
			}

			try
			{
				CommonGraphEditorGraph editorGraph = LoadBaseGraphEditorGraph(editorGraphPath);
				if (editorGraph == null)
				{
					Debug.LogError(
						$"[CommonGraphRuntimeExporter] Could not load CommonGraphEditorGraph at '{editorGraphPath}'.");
					return false;
				}

				GraphCompiler effectiveCompiler = compiler ?? CommonGraphImporter.BuildBaseGraph;

				CommonGraph compiledGraph = effectiveCompiler(editorGraph);
				if (compiledGraph == null)
					// Fresh graph assets can be empty right after creation. Skip auto-build quietly until authoring is valid.
				{
					return false;
				}

				if (s_graphValidationHook != null &&
				    !s_graphValidationHook(editorGraph, compiledGraph, editorGraphPath))
				{
					Object.DestroyImmediate(compiledGraph);
					return false;
				}

				string runtimeGraphPath = GetRuntimeAssetPath(editorGraphPath);
				var existingRuntimeGraph = AssetDatabase.LoadAssetAtPath<CommonGraph>(runtimeGraphPath);

				if (existingRuntimeGraph == null)
				{
					compiledGraph.name = Path.GetFileNameWithoutExtension(runtimeGraphPath);
					AssetDatabase.CreateAsset(compiledGraph, runtimeGraphPath);
					EditorUtility.SetDirty(compiledGraph);
				}
				else
				{
					existingRuntimeGraph.startNodeId = compiledGraph.startNodeId;
					existingRuntimeGraph.nodes =
						new List<BaseGraphNode>(compiledGraph.nodes ?? new List<BaseGraphNode>());
					existingRuntimeGraph.InvalidateLookup();
					EditorUtility.SetDirty(existingRuntimeGraph);
					Object.DestroyImmediate(compiledGraph);
				}

				return true;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return false;
			}
		}

		public static int BuildRuntimeGraphAssets(IEnumerable<string> editorGraphPaths, bool saveAndRefresh)
		{
			return BuildRuntimeGraphAssets(editorGraphPaths, saveAndRefresh, null);
		}

		public static int BuildRuntimeGraphAssets(
			IEnumerable<string> editorGraphPaths,
			bool saveAndRefresh,
			GraphCompiler compiler)
		{
			if (editorGraphPaths == null)
			{
				return 0;
			}

			var builtCount = 0;
			foreach (string path in editorGraphPaths)
			{
				if (!IsBaseGraphAssetPath(path))
				{
					continue;
				}

				if (BuildRuntimeGraphAsset(path, compiler))
				{
					builtCount++;
				}
			}

			if (saveAndRefresh)
			{
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}

			return builtCount;
		}

		public static bool IsBaseGraphAssetPath(string assetPath)
		{
			return !string.IsNullOrWhiteSpace(assetPath)
			       && assetPath.EndsWith($".{CommonGraphEditorGraph.AssetExtension}",
				       StringComparison.OrdinalIgnoreCase);
		}

		public static bool IsValidBaseGraphEditorAsset(string assetPath)
		{
			if (!IsBaseGraphAssetPath(assetPath))
			{
				return false;
			}

			CommonGraphEditorGraph editorGraph = LoadBaseGraphEditorGraph(assetPath);
			return editorGraph != null;
		}

		public static string GetRuntimeAssetPathForEditorGraph(string editorGraphPath)
		{
			return GetRuntimeAssetPath(editorGraphPath);
		}

		private static CommonGraphEditorGraph LoadBaseGraphEditorGraph(string editorGraphPath)
		{
			if (!IsBaseGraphAssetPath(editorGraphPath))
			{
				return null;
			}

			try
			{
				return GraphDatabase.LoadGraph<CommonGraphEditorGraph>(editorGraphPath);
			}
			catch
			{
				return null;
			}
		}

		private static string[] GetSelectedBaseGraphPaths()
		{
			string[] assetGuids = Selection.assetGUIDs;
			var paths = new List<string>(assetGuids.Length);

			for (var i = 0; i < assetGuids.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
				if (string.IsNullOrWhiteSpace(path))
				{
					continue;
				}

				if (!IsBaseGraphAssetPath(path))
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
}