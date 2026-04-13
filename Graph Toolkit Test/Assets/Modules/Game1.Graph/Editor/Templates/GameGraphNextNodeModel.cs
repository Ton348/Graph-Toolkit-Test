using System;
using Unity.GraphToolkit.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public abstract class GameGraphNextNodeModel : GameGraphEditorNode
{
	protected override string DefaultTitle => "Game Next Node";
	protected override string DefaultDescription => "Base template for game node with single next output.";

	protected override void OnDefinePorts(IPortDefinitionContext context)
	{
		AddInputExecutionPort(context);
		AddNextPort(context);
	}
}
