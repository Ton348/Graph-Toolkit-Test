using GraphCore.Editor;
using Game1.Graph.Editor;
using Game1.Graph.Runtime;
using Unity.GraphToolkit.Editor;
using UnityEditor;

using Game1.Graph.Editor.Infrastructure.Bootstrap;
using Game1.Graph.Editor.Infrastructure.Validation;
using GraphCore.Runtime;
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

	private static bool ValidateBeforeBuild(CommonGraphEditorGraph editorGraph, CommonGraph runtimeGraph, string editorGraphPath)
	{
		GameGraphModule module = GameGraphEditorBootstrap.Module;
		if (module == null)
		{
			return true;
		}

		return GameGraphBuildValidationBridge.ValidateBeforeBuild(editorGraph, runtimeGraph, editorGraphPath, module.ValidationComposition);
	}
}
