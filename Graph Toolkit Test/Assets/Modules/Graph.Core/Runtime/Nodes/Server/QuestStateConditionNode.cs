using System;
using GraphCore.Runtime.Templates;

namespace GraphCore.Runtime.Nodes.Server
{
	[Serializable]
	public sealed class QuestStateConditionNode : CoreGraphTrueFalseNode
	{
		public string questId;
		public QuestState state;

		public QuestStateConditionNode()
		{
			Title = "Quest State Condition";
			Description = "Checks quest state and branches";
		}
	}
}