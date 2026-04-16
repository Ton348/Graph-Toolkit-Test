using System;
using Graph.Core.Editor;
using Unity.GraphToolkit.Editor;

namespace Game1.Graph.Editor.Templates
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public abstract class GameGraphSuccessFailNodeModel : GameGraphEditorNode
	{
		protected override string defaultTitle => "Game Success/Fail Node";
		protected override string defaultDescription => "Base template for game node with success/fail branching.";

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			AddSuccessFailPorts(context);
		}
	}
}