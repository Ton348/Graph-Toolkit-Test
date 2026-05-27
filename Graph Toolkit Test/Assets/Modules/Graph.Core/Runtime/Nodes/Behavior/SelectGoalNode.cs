using System;

namespace Graph.Core.Runtime.Nodes.Behavior
{
	[Serializable]
	public sealed class SelectGoalNode : BaseGraphNode
	{
		public BehaviorNeedType needType = BehaviorNeedType.Hunger;
		public float needThreshold = 75f;
		public int workHourFrom = 9;
		public int workHourTo = 21;
		public string goWorkNodeId;
		public string goFoodNodeId;
		public string goHomeNodeId;
		public string wanderNodeId;

		public SelectGoalNode()
		{
			Title = "Выбрать цель";
			Description = "Анализирует текущее состояние NPC и выбирает приоритетную цель поведения: работа, еда, отдых и т.д.";
		}
	}
}
