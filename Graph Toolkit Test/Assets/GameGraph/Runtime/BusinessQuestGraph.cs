using System.Collections.Generic;
using System.Reflection;
using Graph.Core.Runtime;
using Graph.Core.Runtime.Nodes.Server;
using Graph.Core.Runtime.Templates;
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

		if (node is CoreGraphNextNode nextNode)
		{
			return GetNodeById(nextNode.nextNodeId);
		}

		FieldInfo field = node.GetType().GetField("nextNodeId", BindingFlags.Public | BindingFlags.Instance);
		if (field != null && field.FieldType == typeof(string))
		{
			return GetNodeById(field.GetValue(node) as string);
		}

		return null;
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
