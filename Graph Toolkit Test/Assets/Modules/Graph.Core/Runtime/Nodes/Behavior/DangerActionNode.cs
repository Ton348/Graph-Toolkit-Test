using System;

namespace Graph.Core.Runtime.Nodes.Behavior
{
	[Serializable]
	public sealed class DangerActionNode : BaseGraphNode
	{
		public DangerActionType actionType = DangerActionType.RunFromDanger;
		public float value = 30f;
		public float speedDelta = 0f;
		public string successNodeId;

		public DangerActionNode()
		{
			Title = "Действие при опасности";
			Description = "Выполняет движение или freeze действие для danger поведения NPC.";
		}
	}
}
