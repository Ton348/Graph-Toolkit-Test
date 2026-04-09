using System.Collections.Generic;
using System;
using UnityEngine;

public sealed class BaseGraph : ScriptableObject
{
    private const string LogPrefix = "[BaseGraph]";
    public string startNodeId;
    public List<BusinessQuestNode> nodes = new List<BusinessQuestNode>();

    private Dictionary<string, BusinessQuestNode> m_nodeLookup;
    private bool m_lookupInitialized;
    private GraphValidationResult m_lastValidationResult;

    public GraphValidationResult ValidateGraph()
    {
        m_lastValidationResult = BaseGraphValidator.Validate(this);
        return m_lastValidationResult;
    }

    public bool HasValidationErrors()
    {
        return ValidateGraph().ErrorCount > 0;
    }

    public bool TryGetNodeById(string nodeId, out BusinessQuestNode node)
    {
        EnsureLookup();

        if (string.IsNullOrWhiteSpace(nodeId))
        {
            node = null;
            return false;
        }

        if (m_nodeLookup == null)
        {
            node = null;
            return false;
        }

        return m_nodeLookup.TryGetValue(nodeId, out node);
    }

    public BusinessQuestNode GetNodeById(string nodeId)
    {
        return TryGetNodeById(nodeId, out BusinessQuestNode node) ? node : null;
    }

    public bool TryGetStartNode(out BusinessQuestNode node)
    {
        return TryGetNodeById(startNodeId, out node);
    }

    public void InvalidateLookup()
    {
        m_lookupInitialized = false;
        m_nodeLookup = null;
        m_lastValidationResult = null;
    }

    internal Dictionary<string, BusinessQuestNode> BuildNodeLookup(GraphValidationResult validationResult)
    {
        Dictionary<string, BusinessQuestNode> lookup = new Dictionary<string, BusinessQuestNode>(nodes != null ? nodes.Count : 0, StringComparer.Ordinal);

        if (nodes == null)
        {
            validationResult?.AddError("Graph nodes collection is null.");
            return lookup;
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            BusinessQuestNode node = nodes[i];
            if (node == null)
            {
                validationResult?.AddWarning($"Node index {i} is null.", nodeIndex: i);
                continue;
            }

            if (string.IsNullOrWhiteSpace(node.Id))
            {
                validationResult?.AddError($"Node at index {i} has empty id.", nodeIndex: i);
                continue;
            }

            if (!lookup.TryAdd(node.Id, node))
            {
                validationResult?.AddError($"Duplicate node id detected: '{node.Id}'.", node.Id, node.GetType().Name, nameof(node.nodeId), i);
            }
        }

        return lookup;
    }

    private void EnsureLookup()
    {
        if (m_lookupInitialized)
        {
            return;
        }

        GraphValidationResult validation = new GraphValidationResult();
        m_nodeLookup = BuildNodeLookup(validation);
        m_lastValidationResult = validation;
        m_lookupInitialized = true;

        LogValidationIssues(validation);
    }

    private void LogValidationIssues(GraphValidationResult validationResult)
    {
        if (validationResult == null)
        {
            return;
        }

        for (int i = 0; i < validationResult.Errors.Count; i++)
        {
            Debug.LogError($"{LogPrefix} {validationResult.Errors[i]}", this);
        }

        for (int i = 0; i < validationResult.Warnings.Count; i++)
        {
            Debug.LogWarning($"{LogPrefix} {validationResult.Warnings[i]}", this);
        }
    }
}
