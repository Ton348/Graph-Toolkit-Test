using System;

namespace Graph.Core.Runtime.Nodes.Behavior
{
	[Serializable]
	public sealed class CombatActionNode : BaseGraphNode
	{
		public CombatActionType actionType = CombatActionType.FindThreatSource;
		public float value = 10f;
		public string successNodeId;
		public string failNodeId;

		public CombatActionNode()
		{
			Title = "Боевое действие";
			Description = "Выполняет боевое действие NPC через текущий combat context.";
		}
	}
}
