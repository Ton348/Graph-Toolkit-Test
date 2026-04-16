using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

namespace Game1.Graph.Editor.Templates
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public abstract class GameGraphNextNodeModel : GameGraphEditorNode
	{
		protected override string defaultTitle => "Game Next Node";
		protected override string defaultDescription => "Base template for game node with single next output.";

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			AddNextPort(context);
		}
	}
}