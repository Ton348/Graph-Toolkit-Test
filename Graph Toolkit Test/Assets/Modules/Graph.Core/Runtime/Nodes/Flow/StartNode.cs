using System;
using Graph.Core.Runtime.Templates;

namespace Graph.Core.Runtime.Nodes.Flow
{
	[Serializable]
	public sealed class StartNode : CoreGraphNextNode
	{
		public StartNode()
		{
			Title = "Start";
			Description = "Entry point of the graph";
		}
	}
}