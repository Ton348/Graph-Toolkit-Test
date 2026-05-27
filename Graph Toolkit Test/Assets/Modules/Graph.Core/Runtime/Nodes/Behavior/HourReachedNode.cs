using System;

namespace Graph.Core.Runtime.Nodes.Behavior
{
	[Serializable]
	public sealed class HourReachedNode : BaseGraphNode
	{
		public int hour;
		public string nextNodeId;

		public HourReachedNode()
		{
			Title = "Ожидание часа";
			Description = "Ожидает наступления указанного игрового часа.";
		}
	}
}
