using Game1.Graph.Runtime;
using GraphCore.Editor;
using System;
using Unity.GraphToolkit.Editor;

namespace Game1.Graph.Editor
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public sealed class SampleNodeModel : GameGraphNextNodeModel
	{
		public const string EnabledOption = "Enabled";

		protected override string DefaultTitle => "Sample Node";
		protected override string DefaultDescription => "Example game node model for extension workflow.";

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			base.OnDefineOptions(context);
			AddBoolOption(context, EnabledOption, "Enabled", true);
		}
	}
}
