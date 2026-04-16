using System;
using Game1.Graph.Runtime.Templates;

namespace GameGraph.Runtime.Business
{
	[Serializable]
	public sealed class RequestUnlockContactNode : GameGraphSuccessFailNode
	{
		public string contactId;
	}
}