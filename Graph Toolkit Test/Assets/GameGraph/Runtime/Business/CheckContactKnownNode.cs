using System;
using Game1.Graph.Runtime.Templates;

namespace GameGraph.Runtime.Business
{
	[Serializable]
	public sealed class CheckContactKnownNode : GameGraphTrueFalseNode
	{
		public string contactId;
	}
}