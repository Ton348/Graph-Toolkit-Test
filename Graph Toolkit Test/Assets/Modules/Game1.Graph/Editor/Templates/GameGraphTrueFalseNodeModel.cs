using System;
using Graph.Core.Editor;
using Unity.GraphToolkit.Editor;

namespace Game1.Graph.Editor.Templates
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public abstract class GameGraphTrueFalseNodeModel : GameGraphEditorNode
	{
		protected override string defaultTitle => "Game True/False Node";
		protected override string defaultDescription => "Base template for game node with true/false branching.";

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			AddTrueFalsePorts(context);
		}
	}
}