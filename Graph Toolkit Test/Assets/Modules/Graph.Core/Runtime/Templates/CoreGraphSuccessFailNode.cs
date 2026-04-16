using System;

namespace GraphCore.Runtime.Templates
{
	[Serializable]
	public abstract class CoreGraphSuccessFailNode : CoreGraphNode
	{
		public string successNodeId;
		public string failNodeId;
	}
}