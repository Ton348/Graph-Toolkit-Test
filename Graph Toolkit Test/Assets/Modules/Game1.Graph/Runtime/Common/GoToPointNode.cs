using System;
using UnityEngine;

[Serializable]
public class GoToPointNode : BaseGraphNode
{
	public string markerId;
	public Transform targetTransform;
	public float arrivalDistance = 2f;
}
