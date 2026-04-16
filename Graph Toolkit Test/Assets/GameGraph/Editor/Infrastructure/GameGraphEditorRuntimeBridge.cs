using Game1.Graph.Editor.Infrastructure.Bootstrap;
using Game1.Graph.Editor.Infrastructure.Validation;
using Graph.Core.Editor;
using Graph.Core.Runtime;
using UnityEditor;

namespace GameGraph.Editor.Infrastructure
{
	[InitializeOnLoad]
	public static class GameGraphEditorRuntimeBridge
	{
		static GameGraphEditorRuntimeBridge()
		{
			EditorApplication.delayCall += Initialize;
		}

		private static void Initialize()
		{
			EditorApplication.delayCall -= Initialize;

			CommonGraphRuntimeAutoBuilder.SetGraphCompiler(GameGraphRuntimeCompiler.Build);
			CommonGraphRuntimeExporter.SetGraphValidationHook(ValidateBeforeBuild);
		}

		private static bool ValidateBeforeBuild(
			CommonGraphEditorGraph editorGraph,
			CommonGraph runtimeGraph,
			string editorGraphPath)
		{
			GameGraphModule module = GameGraphEditorBootstrap.Module;
			if (module == null)
			{
				return true;
			}

			return GameGraphBuildValidationBridge.ValidateBeforeBuild(editorGraph, runtimeGraph, editorGraphPath,
				module.ValidationComposition);
		}
	}
}