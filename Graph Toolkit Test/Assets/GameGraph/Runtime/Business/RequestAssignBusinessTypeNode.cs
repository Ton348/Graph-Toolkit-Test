using System;
using Game1.Graph.Runtime.Templates;

namespace GameGraph.Runtime.Business
{
	[Serializable]
	public sealed class RequestAssignBusinessTypeNode : GameGraphSuccessFailNode
	{
		public string lotId;
		public string businessTypeId;
	}
}