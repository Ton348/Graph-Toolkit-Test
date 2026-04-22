using System;
using Graph.Core.Runtime.Templates;

namespace Graph.Core.Runtime.Nodes.Server
{
	[Serializable]
	public sealed class StartQuestNode : CoreGraphSuccessFailNode
	{
		public string questId;

		public StartQuestNode()
		{
			Title = "Активировать квест";
			Description = "Активирует квест в профиле игрока";
		}
	}
}
