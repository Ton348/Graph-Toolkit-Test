using System;
using Game1.Graph.Runtime.Templates;
using UnityEngine;

namespace GameGraph.Runtime.Common
{
	[Serializable]
	public sealed class GoToPointNode : GameGraphNextNode
	{
		public string markerId;
		public Transform targetTransform;
		public float arrivalDistance = 2f;
	}
}