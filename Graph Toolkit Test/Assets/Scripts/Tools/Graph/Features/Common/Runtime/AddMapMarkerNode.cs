using System;
using UnityEngine;

[Serializable]
public class AddMapMarkerNode : BusinessQuestNode
{
    public string markerId;
    public Transform targetTransform;
    public string title;
}
