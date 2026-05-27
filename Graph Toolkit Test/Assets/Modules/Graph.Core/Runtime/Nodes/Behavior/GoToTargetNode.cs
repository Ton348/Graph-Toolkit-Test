using System;

namespace Graph.Core.Runtime.Nodes.Behavior
{
	[Serializable]
	public sealed class GoToTargetNode : BaseGraphNode
	{
		public BehaviorTargetType targetType = BehaviorTargetType.Work;
		public string nextNodeId;

		public GoToTargetNode()
		{
			Title = "Перейти к цели";
			Description = "Отправляет NPC к целевой точке, соответствующей выбранному типу цели.";
		}
	}
}
