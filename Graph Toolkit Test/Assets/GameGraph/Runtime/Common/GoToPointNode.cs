using System;
using UnityEngine;
using Game1.Graph.Runtime;

[Serializable]
public sealed class GoToPointNode : GameGraphNextNode
{
	public string markerId;
	public Transform targetTransform;
	public float arrivalDistance = 2f;
}
