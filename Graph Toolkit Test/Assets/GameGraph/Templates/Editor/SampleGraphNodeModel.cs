using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

// Copy this file, rename class and file, then map options/ports to your runtime node.
[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public abstract class SampleGraphNodeModel : GameGraphEditorNode
{
	public const string MessageOption = "Message";

	protected override string DefaultTitle => "Sample Node";
	protected override string DefaultDescription => "Template node model for game-specific nodes.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		AddStringOption(context, MessageOption, "Message");
	}

	protected override void OnDefinePorts(IPortDefinitionContext context)
	{
		AddInputExecutionPort(context);
		AddNextPort(context);
	}
}
