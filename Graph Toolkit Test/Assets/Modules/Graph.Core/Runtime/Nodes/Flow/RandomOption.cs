using System;

namespace GraphCore.Runtime.Nodes.Flow
{
	[Serializable]
	public sealed class RandomOption
	{
		public float weight = 1f;
		public string nextNodeId;
	}
}
