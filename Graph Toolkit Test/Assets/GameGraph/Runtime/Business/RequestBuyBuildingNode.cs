using System;
using Game1.Graph.Runtime.Templates;
using GameGraph.Runtime.Quest;

namespace GameGraph.Runtime.Business
{
	[Serializable]
	public sealed class RequestBuyBuildingNode : GameGraphSuccessFailNode
	{
		public string buildingId;
		public QuestActionType questAction;
		public string questId;
	}
}