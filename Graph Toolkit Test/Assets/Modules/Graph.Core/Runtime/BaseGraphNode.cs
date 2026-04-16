using System;
using UnityEngine.Serialization;

namespace GraphCore.Runtime
{
	[Serializable]
	public abstract class BaseGraphNode
	{
		[FormerlySerializedAs("id")]
		public string nodeId;

		[FormerlySerializedAs("Title")]
		public string title;

		[FormerlySerializedAs("Description")]
		public string description;

		public string Id => nodeId;

		public string id
		{
			get => nodeId;
			set => nodeId = value;
		}

		public string Title
		{
			get => title;
			set => title = value;
		}

		public string Description
		{
			get => description;
			set => description = value;
		}
	}
}
