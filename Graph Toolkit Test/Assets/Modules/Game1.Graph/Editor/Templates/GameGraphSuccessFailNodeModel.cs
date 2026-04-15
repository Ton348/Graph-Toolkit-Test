using Game1.Graph.Runtime;
using GraphCore.Editor;
using System;
using Unity.GraphToolkit.Editor;

using Game1.Graph.Editor;
namespace Game1.Graph.Editor.Templates
{
	[Serializable]
	[UseWithGraph(typeof(CommonGraphEditorGraph))]
	public abstract class GameGraphSuccessFailNodeModel : GameGraphEditorNode
	{
		protected override string DefaultTitle => "Game Success/Fail Node";
		protected override string DefaultDescription => "Base template for game node with success/fail branching.";

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			AddSuccessFailPorts(context);
		}
	}
}
