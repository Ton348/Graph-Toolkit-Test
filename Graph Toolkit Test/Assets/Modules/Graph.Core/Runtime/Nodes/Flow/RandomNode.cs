using System.Collections.Generic;
using System;
using GraphCore.Runtime;

namespace GraphCore.Runtime.Nodes.Flow
{
	[Serializable]
	public sealed class RandomNode : BaseGraphNode
	{
		public readonly List<RandomOption> options = new List<RandomOption>(4)
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
