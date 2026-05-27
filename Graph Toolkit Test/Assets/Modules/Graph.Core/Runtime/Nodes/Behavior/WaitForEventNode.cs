using System;

namespace Graph.Core.Runtime.Nodes.Behavior
{
	[Serializable]
	public sealed class WaitForEventNode : BaseGraphNode
	{
		public string eventName;
		public string nextNodeId;

		public WaitForEventNode()
		{
			Title = "Ожидание события";
			Description = "Ожидает возникновения указанного runtime-события перед продолжением выполнения.";
		}
	}
}
