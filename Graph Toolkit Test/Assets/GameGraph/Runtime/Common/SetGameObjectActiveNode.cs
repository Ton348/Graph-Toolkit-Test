using System;
using Game1.Graph.Runtime.Templates;
using UnityEngine;

namespace GameGraph.Runtime.Common
{
	[Serializable]
	public sealed class SetGameObjectActiveNode : GameGraphNextNode
	{
		public GameObject targetObject;
		public string siteId;
		public string visualId;
		public bool isActive;
	}
}