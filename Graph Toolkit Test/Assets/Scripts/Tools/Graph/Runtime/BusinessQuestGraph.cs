using System.Collections.Generic;
using UnityEngine;

public class BusinessQuestGraph : ScriptableObject
{
    public string startNodeId;

    [SerializeReference]
    public List<BusinessQuestNode> nodes = new List<BusinessQuestNode>();

    public BusinessQuestNode GetNodeById(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        foreach (BusinessQuestNode node in nodes)
        {
            if (node != null && node.id == id)
            {
                return node;
            }
        }

        return null;
    }

    public BusinessQuestNode GetStartNode()
    {
        return GetNodeById(startNodeId);
    }

    public BusinessQuestNode GetNextNode(BusinessQuestNode node)
    {
        if (node == null)
        {
            return null;
        }

        return GetNodeById(node.nextNodeId);
    }
}
