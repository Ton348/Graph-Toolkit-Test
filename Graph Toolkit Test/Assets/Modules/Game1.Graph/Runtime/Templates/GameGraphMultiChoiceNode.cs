using System.Collections.Generic;
using System;
using GraphCore.Runtime;

using Game1.Graph.Runtime;
namespace Game1.Graph.Runtime.Templates
{
	[Serializable]
	public abstract class GameGraphMultiChoiceNode : GameGraphNode
	{
		public List<GameGraphChoiceBranch> options = new List<GameGraphChoiceBranch>();
	}
}
