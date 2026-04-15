using System;
using GraphCore.Runtime;

using Game1.Graph.Runtime;
namespace Game1.Graph.Runtime.Templates
{
	[Serializable]
	public abstract class GameGraphNextNode : GameGraphNode
	{
		public string nextNodeId;
	}
}
