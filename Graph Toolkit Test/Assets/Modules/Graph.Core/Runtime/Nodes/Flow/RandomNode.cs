using System;
using System.Collections.Generic;

namespace Graph.Core.Runtime.Nodes.Flow
{
	[Serializable]
	public sealed class RandomNode : BaseGraphNode
	{
		public List<RandomOption> options = new(4)
		{
			new RandomOption(),
			new RandomOption(),
			new RandomOption(),
			new RandomOption()
		};

		public RandomNode()
		{
			Title = "Случайный выбор";
			Description = "Выбирает случайную ветку";
		}
	}
}
