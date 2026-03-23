using System;
using UnityEngine;

[Serializable]
public class GoToPointNode : BusinessQuestNode
{
    public string markerId;
    public Transform targetTransform;
    public float arrivalDistance = 2f;
}
