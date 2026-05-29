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
			Description = "Изменяет danger параметры NPC или распространяет danger event.";
		}
	}
}
