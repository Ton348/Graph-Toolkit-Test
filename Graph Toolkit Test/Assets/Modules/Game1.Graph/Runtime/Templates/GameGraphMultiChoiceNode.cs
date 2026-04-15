using System.Collections.Generic;
using System;

namespace Game1.Graph.Runtime
{
	[Serializable]
	public abstract class GameGraphMultiChoiceNode : GameGraphNode
	{
		public List<GameGraphChoiceBranch> options = new List<GameGraphChoiceBranch>();
	}
}
