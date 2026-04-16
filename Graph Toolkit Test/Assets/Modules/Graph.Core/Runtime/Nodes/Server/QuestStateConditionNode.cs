using System;

namespace GraphCore.Runtime.Nodes.Server
{
	[Serializable]
	public sealed class QuestStateConditionNode : BaseGraphNode
	{
		public string questId;
		public QuestState state;
		public string trueNodeId;
		public string falseNodeId;

		public QuestStateConditionNode()
		{
			Title = "Quest State Condition";
			Description = "Checks quest state and branches";
		}
	}
}
