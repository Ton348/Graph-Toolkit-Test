using System;

namespace Graph.Core.Runtime.Nodes.Behavior
{
	[Serializable]
	public sealed class HungerThresholdReachedNode : BaseGraphNode
	{
		public BehaviorNeedType needType = BehaviorNeedType.Hunger;
		public float threshold = 75f;
		public string nextNodeId;

		public HungerThresholdReachedNode()
		{
			Title = "Проверка потребности";
			Description = "Проверяет, достигла ли выбранная потребность заданного порога.";
		}
	}
}
