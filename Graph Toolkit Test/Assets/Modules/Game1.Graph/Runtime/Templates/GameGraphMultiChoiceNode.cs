using System;
using System.Collections.Generic;

namespace Game1.Graph.Runtime.Templates
{
	[Serializable]
	public abstract class GameGraphMultiChoiceNode : GameGraphNode
	{
		public List<GameGraphChoiceBranch> options = new();
	}
}