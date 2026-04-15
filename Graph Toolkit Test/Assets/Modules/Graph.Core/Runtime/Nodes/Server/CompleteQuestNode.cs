using System;
using GraphCore.Runtime;

namespace GraphCore.Runtime.Nodes.Server
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
