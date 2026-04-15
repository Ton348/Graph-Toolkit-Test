using Game1.Graph.Runtime;
using GraphCore.Editor;
using System;
using Unity.GraphToolkit.Editor;

using Game1.Graph.Editor;
namespace Game1.Graph.Editor.Templates
{
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
}
