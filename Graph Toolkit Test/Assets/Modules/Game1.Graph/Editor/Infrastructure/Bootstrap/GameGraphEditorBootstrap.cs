using System;
using System.Collections.Generic;
using System.Reflection;
using Game1.Graph.Editor.Infrastructure.Validation;
using Game1.Graph.Runtime;
using Graph.Core.Editor;
using Graph.Core.Runtime;
using Unity.GraphToolkit.Editor;
using UnityEditor;

namespace Game1.Graph.Editor.Infrastructure.Bootstrap
{
	[InitializeOnLoad]
	public static class GameGraphEditorBootstrap
	{
		static GameGraphEditorBootstrap()
		{
			Initialize();
		}

		public static GameGraphModule Module { get; private set; }

		private static void Initialize()
		{
			Module = GameGraphModule.Create()
				.WithAutoRegistration(GetEditorAssemblies())
				.Build();

			CommonGraphImporter.SetExternalConverter(ConvertExternalNode);
			CommonGraphRuntimeExporter.SetGraphValidationHook(ValidateBeforeBuild);
		}

		private static Assembly[] GetEditorAssemblies()
		{
			var assemblies = new List<Assembly>();
			Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (var i = 0; i < loadedAssemblies.Length; i++)
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
			if (Module == null || editorNode == null)
			{
				return null;
			}

			if (Module.EditorComposition == null || Module.EditorComposition.ConverterRegistry == null)
			{
				return null;
			}

			if (Module.EditorComposition.ConverterRegistry.TryConvert(editorNode, out GameGraphNode runtimeNode))
			{
				return runtimeNode;
			}

			return null;
		}

		private static bool ValidateBeforeBuild(
			CommonGraphEditorGraph editorGraph,
			CommonGraph runtimeGraph,
			string editorGraphPath)
		{
			if (Module == null)
			{
				return true;
			}

			return GameGraphBuildValidationBridge.ValidateBeforeBuild(editorGraph, runtimeGraph, editorGraphPath,
				Module.ValidationComposition);
		}
	}
}