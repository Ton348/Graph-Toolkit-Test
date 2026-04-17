using System;
using System.Collections.Generic;

namespace Graph.Core.Runtime.Nodes.Flow
{
	[Serializable]
	public sealed class RandomNode : BaseGraphNode
	{
		public readonly List<RandomOption> options = new(4)
		{
			new RandomOption(),
			new RandomOption(),
			new RandomOption(),
			new RandomOption()
		};

		public RandomNode()
		{
			Title = "Random";
			Description = "Chooses a weighted random branch";
		}
	}
}