using System;
using Game1.Graph.Runtime.Templates;

namespace GameGraph.Runtime.Business
{
	[Serializable]
	public sealed class RequestSetBusinessMarkupNode : GameGraphSuccessFailNode
	{
		public string lotId;
		public int markupPercent;
	}
}