using System;

namespace Graph.Core.Runtime.Nodes.UI
{
	[Serializable]
	public sealed class ChoiceOption
	{
		public string label;
		public string nextNodeId;
	}
}