using System;

namespace GraphCore.BaseNodes.Runtime.World
{
	[Serializable]
	public sealed class MapMarkerNode : BaseGraphNode
	{
		public string markerId;
		public string targetObjectName;

		public MapMarkerNode()
		{
			Title = "Map Marker";
			Description = "Shows or updates a map marker";
		}
	}
}
