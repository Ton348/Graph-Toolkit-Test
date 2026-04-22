using System;
using Game1.Graph.Runtime.Templates;
using GameGraph.Runtime.Business;
using Graph.Core.Runtime;

namespace GameGraph.Runtime.Business
{
	[Serializable]
	public sealed class RequestBuyTraderItemNode : GameGraphSuccessFailNode
	{
		public string itemId;
	}
}
