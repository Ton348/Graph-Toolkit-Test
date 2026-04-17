using System;
using Game1.Graph.Runtime.Templates;

namespace GameGraph.Runtime.Business
{
	[Serializable]
	public sealed class RequestTradeOfferNode : GameGraphSuccessFailNode
	{
		public string buildingId;
	}
}