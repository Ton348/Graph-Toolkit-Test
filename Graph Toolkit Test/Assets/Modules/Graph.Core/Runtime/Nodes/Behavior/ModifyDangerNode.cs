using System;

namespace Graph.Core.Runtime.Nodes.Behavior
{
	[Serializable]
	public sealed class ModifyDangerNode : BaseGraphNode
	{
		public DangerModifyMode mode = DangerModifyMode.SetThreatScore;
		public float value = 1f;
		public string nextNodeId;

		public ModifyDangerNode()
		{
			Title = "Изменить опасность";
			Description =
				"Mode:\n" +
				"EnableSpreadDanger — если Value > 0, отправляет danger event от NPC.\n" +
				"SetDangerRadius — изменяет радиус опасности NPC на Value.\n" +
				"SetDangerTimer — изменяет таймер опасности NPC на Value.\n" +
				"SetThreatScore — изменяет threat score NPC на Value.";
		}
	}
}
