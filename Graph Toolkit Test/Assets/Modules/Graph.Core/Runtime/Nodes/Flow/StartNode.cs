using System;

namespace GraphCore.Runtime.Nodes.Flow
{
	[Serializable]
	public sealed class StartNode : BaseGraphNode
	{
		public StartNode()
		{
			Title = "Start";
			Description = "Entry point of the graph";
		}
	}
}
