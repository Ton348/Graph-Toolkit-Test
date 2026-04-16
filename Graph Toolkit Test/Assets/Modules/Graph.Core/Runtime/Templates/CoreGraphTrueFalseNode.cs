using System;

namespace GraphCore.Runtime.Templates
{
	[Serializable]
	public abstract class CoreGraphTrueFalseNode : CoreGraphNode
	{
		public string trueNodeId;
		public string falseNodeId;
	}
}
