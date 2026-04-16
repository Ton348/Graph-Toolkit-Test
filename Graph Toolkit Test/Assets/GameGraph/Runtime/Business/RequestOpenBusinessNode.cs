using System;
using Game1.Graph.Runtime.Templates;

namespace GameGraph.Runtime.Business
{
	[Serializable]
	public sealed class RequestOpenBusinessNode : GameGraphSuccessFailNode
	{
		public string lotId;
	}
}