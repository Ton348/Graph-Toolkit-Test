using System;
using Graph.Core.Runtime.Templates;

namespace Graph.Core.Runtime.Nodes.Flow
{
	[Serializable]
	public sealed class DelayNode : CoreGraphNextNode
	{
		public float delaySeconds = 1f;

		public DelayNode()
		{
			Title = "Задержка";
			Description = "Останавливает выполнение следующих нод на время";
		}
	}
}
