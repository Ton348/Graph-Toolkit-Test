using System;
using UnityEngine.Serialization;

[Serializable]
public abstract class BaseGraphNode
{
	[FormerlySerializedAs("id")]
	public string nodeId;

	[FormerlySerializedAs("nextNodeId")]
	public string nextNodeId;

	[FormerlySerializedAs("Title")]
	public string title;

	[FormerlySerializedAs("Description")]
	public string description;

	public string Id => nodeId;

	public string id
	{
		get { return nodeId; }
		set { nodeId = value; }
	}

	public string Title
	{
		get { return title; }
		set { title = value; }
	}

	public string Description
	{
		get { return description; }
		set { description = value; }
	}

}
