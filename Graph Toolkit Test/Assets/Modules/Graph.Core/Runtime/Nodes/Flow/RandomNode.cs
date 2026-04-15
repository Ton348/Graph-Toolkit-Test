using System.Collections.Generic;
using System;

namespace GraphCore.BaseNodes.Runtime.Flow
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
