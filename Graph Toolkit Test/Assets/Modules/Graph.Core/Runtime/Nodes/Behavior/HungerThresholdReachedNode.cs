using System;

namespace Graph.Core.Runtime.Nodes.Behavior
{
	[Serializable]
	public sealed class HungerThresholdReachedNode : BaseGraphNode
	{
		public BehaviorNeedType needType = BehaviorNeedType.Hunger;
		public float threshold = 75f;
		public string trueNodeId;
		public string falseNodeId;

		public HungerThresholdReachedNode()
		{
			Title = "Проверка характеристики";
			Description = "Проверяет, достигла ли выбранная характеристика заданного порога.";
		}
	}
}
