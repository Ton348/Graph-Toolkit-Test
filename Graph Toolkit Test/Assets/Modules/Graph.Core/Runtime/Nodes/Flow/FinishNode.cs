using System;
using GraphCore.Runtime;

namespace GraphCore.Runtime.Nodes.Flow
{
	[Serializable]
	public sealed class FinishNode : BaseGraphNode
	{
		public FinishNode()
		{
			Title = "Finish";
			Description = "Terminates graph execution";
		}
	}
}
