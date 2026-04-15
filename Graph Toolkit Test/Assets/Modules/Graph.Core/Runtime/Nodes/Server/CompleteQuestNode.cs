using System;

namespace GraphCore.BaseNodes.Runtime.Server
{
	[Serializable]
	public sealed class CompleteQuestNode : BaseGraphNode
	{
		public string questId;
		public string successNodeId;
		public string failNodeId;

		public CompleteQuestNode()
		{
			Title = "Complete Quest";
			Description = "Requests quest completion";
		}
	}
}
