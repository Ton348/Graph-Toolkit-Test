using System;
using UnityEngine;

[Serializable]
public sealed class GoToPointNode : GameGraphNextNode
{
	public string markerId;
	public Transform targetTransform;
	public float arrivalDistance = 2f;
}
