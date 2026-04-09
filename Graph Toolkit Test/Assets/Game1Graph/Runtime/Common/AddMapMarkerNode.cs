using System;
using UnityEngine;

[Serializable]
public class AddMapMarkerNode : BaseGraphNode
{
    public string markerId;
    public Transform targetTransform;
    public string title;
}
