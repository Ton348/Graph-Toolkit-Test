using System;
using GraphCore.Runtime.Templates;

namespace GraphCore.Runtime.Nodes.Server
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
