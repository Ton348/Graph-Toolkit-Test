using System;

namespace Game1.Graph.Runtime.Templates
{
	[Serializable]
	public abstract class GameGraphSuccessFailNode : GameGraphNode
	{
		public string successNodeId;
		public string failNodeId;
	}
}