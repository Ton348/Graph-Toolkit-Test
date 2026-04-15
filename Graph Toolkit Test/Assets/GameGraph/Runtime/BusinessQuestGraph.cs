using System.Collections.Generic;
using GraphCore.Runtime;
using GraphCore.Runtime.Nodes.Server;
using UnityEngine;

public class BusinessQuestGraph : ScriptableObject
{
	public string startNodeId;

	[SerializeReference]
	public List<BaseGraphNode> nodes = new List<BaseGraphNode>();

	public BaseGraphNode GetNodeById(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}

		foreach (BaseGraphNode node in nodes)
		{
			if (node != null && node.id == id)
			{
				return node;
			}
		}

		return null;
	}

	public BaseGraphNode GetStartNode()
	{
		return GetNodeById(startNodeId);
	}

	public BaseGraphNode GetNextNode(BaseGraphNode node)
	{
		if (node == null)
		{
			return null;
		}

		return GetNodeById(node.nextNodeId);
	}

	public CheckpointNode GetCheckpointNodeById(string checkpointId)
	{
		if (string.IsNullOrEmpty(checkpointId))
		{
			return null;
		}

		foreach (BaseGraphNode node in nodes)
		{
			if (node is CheckpointNode checkpoint && checkpoint.checkpointId == checkpointId)
			{
				return checkpoint;
			}
		}

		return null;
	}
}
