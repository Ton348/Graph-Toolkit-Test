using System;

namespace GraphCore.BaseNodes.Runtime.Flow
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
