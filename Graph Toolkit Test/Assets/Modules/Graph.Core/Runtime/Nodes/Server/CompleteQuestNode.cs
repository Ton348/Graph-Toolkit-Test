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
			Title = "Complete Quest";
			Description = "Requests quest completion";
		}
	}
}