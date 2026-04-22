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
			Title = "Проверка квеста";
			Description = "Проверяет состояние квеста";
		}
	}
}
