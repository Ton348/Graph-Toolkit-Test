using System;
using GraphCore.Runtime;

using Game1.Graph.Runtime;
namespace Game1.Graph.Runtime.Templates
{
	[Serializable]
	public abstract class GameGraphTrueFalseNode : GameGraphNode
	{
		public string trueNodeId;
		public string falseNodeId;
	}
}
