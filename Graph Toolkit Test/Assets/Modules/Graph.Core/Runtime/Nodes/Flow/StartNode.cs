using System;
using GraphCore.Runtime.Templates;

namespace GraphCore.Runtime.Nodes.Flow
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