using System;
using Game1.Graph.Editor;
using Game1.Graph.Runtime.Infrastructure;
using Unity.GraphToolkit.Editor;

namespace GameGraph.Editor.Business
{
	[Serializable]
	public abstract class GameGraphSuccessFailNodeModel : GameGraphEditorNode
	{
		public const string SuccessPort = GameGraphPortNames.Success;
		public const string FailPort = GameGraphPortNames.Fail;

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			AddInputExecutionPort(context);
			context.AddOutputPort(SuccessPort).WithDisplayName(SuccessPort).WithConnectorUI(PortConnectorUI.Arrowhead)
				.Build();
			context.AddOutputPort(FailPort).WithDisplayName(FailPort).WithConnectorUI(PortConnectorUI.Arrowhead).Build();
		}
	}
}