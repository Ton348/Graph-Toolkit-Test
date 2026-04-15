using Game1.Graph.Runtime;
using GraphCore.Editor;
using System;

using Game1.Graph.Editor.Infrastructure;
using Game1.Graph.Editor.Infrastructure.Import;
using Game1.Graph.Runtime.Infrastructure;
using Game1.Graph.Runtime.Infrastructure.Validation;
namespace Game1.Graph.Editor.Infrastructure.Validation
{
	public sealed class GameGraphModule
	{
		public GameGraphComposition RuntimeComposition { get; }
		public GameGraphEditorComposition EditorComposition { get; }
		public GameGraphValidationComposition ValidationComposition { get; }

		internal GameGraphModule(
			GameGraphComposition runtimeComposition,
			GameGraphEditorComposition editorComposition,
			GameGraphValidationComposition validationComposition)
		{
			RuntimeComposition = runtimeComposition ?? throw new ArgumentNullException(nameof(runtimeComposition));
			EditorComposition = editorComposition ?? throw new ArgumentNullException(nameof(editorComposition));
			ValidationComposition = validationComposition ?? throw new ArgumentNullException(nameof(validationComposition));
		}

		public GameGraphImporterIntegration CreateImporterIntegration()
		{
			return new GameGraphImporterIntegration(EditorComposition);
		}

		public static GameGraphModuleBuilder Create()
		{
			return new GameGraphModuleBuilder();
		}
	}
}
