using System;
using GraphCore.Runtime;
using Game1.Graph.Runtime;

namespace Game1.Graph.Runtime.Templates
{
	[Serializable]
	public sealed class GameGraphChoiceBranch
	{
		public string label;
		public string nextNodeId;
	}
}
