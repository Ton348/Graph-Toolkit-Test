using UnityEditor;
using UnityEngine;
using Graph.Core.Editor;
using Unity.GraphToolkit.Editor;
using System;
using System.Collections.Generic;
using System.Reflection;

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
			.WithAutoRegistration(GetEditorAssemblies())
			.Build();

		CommonGraphImporter.SetExternalConverter(ConvertExternalNode);

		Debug.Log("[GameGraph] Module initialized.");
	}

	private static Assembly[] GetEditorAssemblies()
	{
		List<Assembly> assemblies = new List<Assembly>();
		Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < loadedAssemblies.Length; i++)
		{
			Assembly assembly = loadedAssemblies[i];
			if (assembly == null || assembly.IsDynamic)
			{
				continue;
			}

			string assemblyName = assembly.GetName().Name;
			if (string.IsNullOrWhiteSpace(assemblyName))
			{
				continue;
			}

			if (!assemblyName.EndsWith(".Editor", StringComparison.Ordinal) &&
				!string.Equals(assemblyName, "Assembly-CSharp-Editor", StringComparison.Ordinal))
			{
				continue;
			}

			assemblies.Add(assembly);
		}

		return assemblies.ToArray();
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
