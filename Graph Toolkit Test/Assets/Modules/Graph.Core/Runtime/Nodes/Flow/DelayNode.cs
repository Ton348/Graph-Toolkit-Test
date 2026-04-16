using System;
using GraphCore.Runtime.Templates;

namespace GraphCore.Runtime.Nodes.Flow
{
	[Serializable]
	public sealed class DelayNode : CoreGraphNextNode
	{
		public float delaySeconds = 1f;

		public DelayNode()
		{
			Title = "Delay";
			Description = "Waits for a duration and continues";
		}
	}
}
