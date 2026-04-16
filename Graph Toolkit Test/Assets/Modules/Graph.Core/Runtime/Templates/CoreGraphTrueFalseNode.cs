using System;

namespace Graph.Core.Runtime.Templates
{
	[Serializable]
	public abstract class CoreGraphTrueFalseNode : CoreGraphNode
	{
		public string trueNodeId;
		public string falseNodeId;
	}
}