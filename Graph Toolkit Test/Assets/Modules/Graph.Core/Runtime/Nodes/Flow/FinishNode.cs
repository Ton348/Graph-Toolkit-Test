using System;

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