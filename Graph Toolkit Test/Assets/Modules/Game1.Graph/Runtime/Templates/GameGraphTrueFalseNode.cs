using System;

namespace Game1.Graph.Runtime
{
	[Serializable]
	public abstract class GameGraphTrueFalseNode : GameGraphNode
	{
		public string trueNodeId;
		public string falseNodeId;
	}
}
