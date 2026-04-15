using Game1.Graph.Runtime;
using GraphCore.Editor;
using System;
using Unity.GraphToolkit.Editor;

namespace Game1.Graph.Editor
{
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
}
