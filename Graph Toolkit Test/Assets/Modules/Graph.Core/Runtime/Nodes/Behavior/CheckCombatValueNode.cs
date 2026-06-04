using System;

namespace Graph.Core.Runtime.Nodes.Behavior
{
	[Serializable]
	public sealed class CheckCombatValueNode : BaseGraphNode
	{
		public CombatValueType valueType = CombatValueType.ThreatScore;
		public CombatComparisonType comparisonType = CombatComparisonType.Greater;
		public float value = 10f;
		public string trueNodeId;
		public string falseNodeId;

		public CheckCombatValueNode()
		{
			Title = "Проверка боевого значения";
			Description = "Проверяет боевой параметр NPC и выбирает следующую ветку выполнения.";
		}
	}
}
