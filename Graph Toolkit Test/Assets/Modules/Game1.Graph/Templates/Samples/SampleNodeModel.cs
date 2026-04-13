using System;
using Unity.GraphToolkit.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class SampleNodeModel : GameGraphNextNodeModel
{
	public const string ENABLED_OPTION = "Enabled";

	protected override string DefaultTitle => "Sample Node";
	protected override string DefaultDescription => "Example game node model for extension workflow.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		AddBoolOption(context, ENABLED_OPTION, "Enabled", true);
	}
}
