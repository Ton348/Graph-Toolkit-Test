using System;
using UnityEngine.Serialization;

[Serializable]
public abstract class BaseGraphNode
{
	private const string LegacyApiMessage = "Legacy compatibility API. Use serialized fields directly.";

	[FormerlySerializedAs("id")]
	public string nodeId;

	[FormerlySerializedAs("nextNodeId")]
	public string nextNodeId;

	[FormerlySerializedAs("Title")]
	public string title;

	[FormerlySerializedAs("Description")]
	public string description;

	public string Id => nodeId;

	[Obsolete(LegacyApiMessage)]
	public string id
	{
		get { return nodeId; }
		set { nodeId = value; }
	}

	[Obsolete(LegacyApiMessage)]
	public string Title
	{
		get { return title; }
		set { title = value; }
	}

	[Obsolete(LegacyApiMessage)]
	public string Description
	{
		get { return description; }
		set { description = value; }
	}

}
