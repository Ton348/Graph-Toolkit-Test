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
			Title = "Start Quest";
			Description = "Requests quest start";
		}
	}
}