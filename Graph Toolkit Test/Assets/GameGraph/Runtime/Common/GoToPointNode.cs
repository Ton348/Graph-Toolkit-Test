using System;
using UnityEngine;
using Game1.Graph.Runtime;

using Game1.Graph.Runtime.Templates;
[Serializable]
public sealed class GoToPointNode : GameGraphNextNode
{
	public string markerId;
	public Transform targetTransform;
	public float arrivalDistance = 2f;
}
