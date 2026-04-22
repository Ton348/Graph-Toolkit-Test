using System;
using Graph.Core.Runtime.Templates;

namespace Graph.Core.Runtime.Nodes.World
{
	[Serializable]
	public sealed class MapMarkerNode : CoreGraphNextNode
	{
		public string markerId;
		public string targetObjectName;

		public MapMarkerNode()
		{
			Title = "Метка на карте";
			Description = "Добавляет маркер на карту";
		}
	}
}
