using System;

namespace GraphCore.BaseNodes.Runtime.Flow
{
	[Serializable]
	public sealed class RandomOption
	{
		public float weight = 1f;
		public string nextNodeId;
	}
}
