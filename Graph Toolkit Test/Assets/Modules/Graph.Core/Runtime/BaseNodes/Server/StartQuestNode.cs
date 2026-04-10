using System;

namespace GraphCore.BaseNodes.Runtime.Server
{
	[Serializable]
	public sealed class StartQuestNode : BaseGraphNode
	{
		public string questId;
		public string successNodeId;
		public string failNodeId;

		public StartQuestNode()
		{
			Title = "Start Quest";
			Description = "Requests quest start";
		}
	}
}
