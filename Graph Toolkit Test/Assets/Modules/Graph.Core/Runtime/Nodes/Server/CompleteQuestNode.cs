using System;
using Graph.Core.Runtime.Templates;

namespace Graph.Core.Runtime.Nodes.Server
{
	[Serializable]
	public sealed class CompleteQuestNode : CoreGraphSuccessFailNode
	{
		public string questId;

		public CompleteQuestNode()
		{
			Title = "Завершить квест";
			Description = "Завершает квест в профиле игрока";
		}
	}
}
