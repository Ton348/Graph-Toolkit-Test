using System;
using Graph.Core.Runtime.Templates;

namespace Graph.Core.Runtime.Nodes.Server
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