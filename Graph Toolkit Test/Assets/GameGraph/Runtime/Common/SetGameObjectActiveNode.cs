using System;
using UnityEngine;
using Game1.Graph.Runtime;

[Serializable]
public sealed class SetGameObjectActiveNode : GameGraphNextNode
{
	public GameObject targetObject;
	public string siteId;
	public string visualId;
	public bool isActive;
}
