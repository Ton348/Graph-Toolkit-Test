using System;
using UnityEngine;
using Game1.Graph.Runtime;

using Game1.Graph.Runtime.Templates;
[Serializable]
public sealed class SetGameObjectActiveNode : GameGraphNextNode
{
	public GameObject targetObject;
	public string siteId;
	public string visualId;
	public bool isActive;
}
