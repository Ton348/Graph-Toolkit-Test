using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

// Copy this file, rename class and file, then map options/ports to your runtime node.
[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public abstract class SampleGraphNodeModel : GameGraphEditorNode
{
	public const string MESSAGE_OPTION = "Message";

	protected override string DefaultTitle => "Sample Node";
	protected override string DefaultDescription => "Template node model for game-specific nodes.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		AddStringOption(context, MESSAGE_OPTION, "Message");
	}

	protected override void OnDefinePorts(IPortDefinitionContext context)
	{
		AddInputExecutionPort(context);
		AddNextPort(context);
	}
}
