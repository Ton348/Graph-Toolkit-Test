using System.Collections.Generic;
using System;
using UnityEngine;

public sealed class CommonGraph : ScriptableObject
{
	private const string LogPrefix = "[CommonGraph]";
	public string startNodeId;
	[SerializeReference]
	public List<BaseGraphNode> nodes = new List<BaseGraphNode>();

	private Dictionary<string, BaseGraphNode> m_nodeLookup;
	private bool m_lookupInitialized;
	private GraphValidationResult m_lastValidationResult;
	private int m_lastLookupNodeCount = -1;

	public GraphValidationResult ValidateGraph()
	{
		m_lastValidationResult = CommonGraphValidator.Validate(this);
		return m_lastValidationResult;
	}

	public bool HasValidationErrors()
	{
		return ValidateGraph().ErrorCount > 0;
	}

	public bool TryGetNodeById(string nodeId, out BaseGraphNode node)
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

	public BaseGraphNode GetNodeById(string nodeId)
	{
		return TryGetNodeById(nodeId, out BaseGraphNode node) ? node : null;
	}

	public bool TryGetStartNode(out BaseGraphNode node)
	{
		return TryGetNodeById(startNodeId, out node);
	}

	public void InvalidateLookup()
	{
		m_lookupInitialized = false;
		m_nodeLookup = null;
		m_lastValidationResult = null;
		m_lastLookupNodeCount = -1;
	}

	internal Dictionary<string, BaseGraphNode> BuildNodeLookup(GraphValidationResult validationResult)
	{
		Dictionary<string, BaseGraphNode> lookup = new Dictionary<string, BaseGraphNode>(nodes != null ? nodes.Count : 0, StringComparer.Ordinal);

		if (nodes == null)
		{
			validationResult?.AddError("Graph nodes collection is null.");
			return lookup;
		}

		for (int i = 0; i < nodes.Count; i++)
		{
			BaseGraphNode node = nodes[i];
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
		int currentNodeCount = nodes != null ? nodes.Count : 0;
		bool shouldRebuildLookup = !m_lookupInitialized || m_nodeLookup == null || m_lastLookupNodeCount != currentNodeCount;
		if (!shouldRebuildLookup)
		{
			return;
		}

		GraphValidationResult validation = new GraphValidationResult();
		m_nodeLookup = BuildNodeLookup(validation);
		m_lastValidationResult = validation;
		m_lookupInitialized = true;
		m_lastLookupNodeCount = currentNodeCount;

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
	}
}
