using System;
using System.Collections.Generic;
using Game1.Graph.Editor.Infrastructure.AutoRegistration;
using Game1.Graph.Runtime;

namespace Game1.Graph.Editor.Infrastructure.Import
{
	public sealed class GameGraphImporterIntegration
	{
		public GameGraphImporterIntegration(GameGraphEditorComposition editorComposition)
		{
			EditorComposition = editorComposition ?? throw new ArgumentNullException(nameof(editorComposition));
		}

		public GameGraphEditorComposition EditorComposition { get; }

		public bool HasConverterFor(object editorNodeModel)
		{
			return EditorComposition.ConverterRegistry.HasConverterFor(editorNodeModel);
		}

		public bool TryConvert(object editorNodeModel, out GameGraphNode runtimeNode)
		{
			return EditorComposition.TryConvert(editorNodeModel, out runtimeNode);
		}

		public IReadOnlyList<Type> GetMissingConverters(IEnumerable<object> editorNodeModels)
		{
			return GameGraphAutoRegistration.FindUnsupportedModels(editorNodeModels,
				EditorComposition.ConverterRegistry);
		}

		public IReadOnlyList<Type> GetUnsupportedModels(IEnumerable<object> editorNodeModels)
		{
			return GetMissingConverters(editorNodeModels);
		}
	}
}