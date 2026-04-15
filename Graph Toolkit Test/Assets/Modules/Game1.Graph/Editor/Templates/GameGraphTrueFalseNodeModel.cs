using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public abstract class GameGraphTrueFalseNodeModel : GameGraphEditorNode
{
	protected override string DefaultTitle => "Game True/False Node";
	protected override string DefaultDescription => "Base template for game node with true/false branching.";

	protected override void OnDefinePorts(IPortDefinitionContext context)
	{
		AddInputExecutionPort(context);
		AddTrueFalsePorts(context);
	}
}
