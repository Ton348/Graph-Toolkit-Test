using System;
using UnityEngine.Serialization;

namespace GraphCore.Runtime.Templates
{
	[Serializable]
	public abstract class CoreGraphNextNode : CoreGraphNode
	{
		[FormerlySerializedAs("nextNodeId")]
		public string nextNodeId;
	}
}
