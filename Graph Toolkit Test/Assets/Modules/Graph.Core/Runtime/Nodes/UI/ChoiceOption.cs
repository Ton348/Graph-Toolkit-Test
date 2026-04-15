using System;
using GraphCore.Runtime;

namespace GraphCore.Runtime.Nodes.UI
{
	[Serializable]
	public sealed class ChoiceOption
	{
		public string label;
		public string nextNodeId;
	}
}
