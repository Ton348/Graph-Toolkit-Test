using System;

namespace Graph.Core.Runtime.Templates
{
	[Serializable]
	public abstract class CoreGraphSuccessFailNode : CoreGraphNode
	{
		public string successNodeId;
		public string failNodeId;
	}
}