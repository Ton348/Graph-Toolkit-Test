using System;
using Game1.Graph.Runtime.Templates;

namespace GameGraph.Runtime.Business
{
	[Serializable]
	public sealed class RequestSetBusinessOpenNode : GameGraphSuccessFailNode
	{
		public string lotId;
		public bool open;
	}
}