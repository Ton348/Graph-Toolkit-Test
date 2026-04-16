using System;
using GraphCore.Runtime.Templates;

namespace GraphCore.Runtime.Nodes.World
{
	[Serializable]
	public sealed class MapMarkerNode : CoreGraphNextNode
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