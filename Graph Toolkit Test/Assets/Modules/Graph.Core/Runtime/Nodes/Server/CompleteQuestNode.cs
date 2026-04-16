using System;
using GraphCore.Runtime.Templates;

namespace GraphCore.Runtime.Nodes.Server
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