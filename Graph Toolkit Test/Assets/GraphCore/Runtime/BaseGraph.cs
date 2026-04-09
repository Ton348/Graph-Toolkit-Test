using System.Collections.Generic;
using UnityEngine;

public sealed class BaseGraph : ScriptableObject
{
    public string startNodeId;
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
}
