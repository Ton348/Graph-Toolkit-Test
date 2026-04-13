using UnityEditor;
using UnityEngine;
using Graph.Core.Editor;
using Unity.GraphToolkit.Editor;

[InitializeOnLoad]
public static class GameGraphEditorBootstrap
{
	private static GameGraphModule s_module;

	static GameGraphEditorBootstrap()
	{
		Initialize();
	}

	public static GameGraphModule Module => s_module;

	private static void Initialize()
	{
		s_module = GameGraphModule.Create()
			.WithAutoRegistration(typeof(GameGraphEditorBootstrap).Assembly)
			.Build();

		CommonGraphImporter.SetExternalConverter(ConvertExternalNode);

		Debug.Log("[GameGraph] Module initialized.");
	}

	private static BaseGraphNode ConvertExternalNode(INode editorNode)
	{
		if (s_module == null || editorNode == null)
		{
			return null;
		}

		if (s_module.EditorComposition == null || s_module.EditorComposition.ConverterRegistry == null)
		{
			return null;
		}

		if (s_module.EditorComposition.ConverterRegistry.TryConvert(editorNode, out GameGraphNode runtimeNode))
		{
			return runtimeNode;
		}

		return null;
	}
}
