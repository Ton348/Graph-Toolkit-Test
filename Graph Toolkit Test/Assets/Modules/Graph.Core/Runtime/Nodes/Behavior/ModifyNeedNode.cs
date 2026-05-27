using System;

namespace Graph.Core.Runtime.Nodes.Behavior
{
	[Serializable]
	public sealed class ModifyNeedNode : BaseGraphNode
	{
		public BehaviorNeedType needType = BehaviorNeedType.Hunger;
		public float amount = 20f;
		public bool increase;
		public string nextNodeId;

		public ModifyNeedNode()
		{
			Title = "Изменить потребность";
			Description = "Увеличивает или уменьшает значение выбранной потребности NPC.";
		}
	}
}
