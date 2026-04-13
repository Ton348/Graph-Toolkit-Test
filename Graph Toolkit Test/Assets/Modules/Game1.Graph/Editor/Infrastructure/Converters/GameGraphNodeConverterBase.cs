using System;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;

public abstract class GameGraphNodeConverterBase<TModel, TNode> : IGameGraphNodeConverter
	where TModel : Node
	where TNode : GameGraphNode
{
	public bool CanConvert(object editorNodeModel)
	{
		return editorNodeModel is TModel;
	}

	public bool TryConvert(object editorNodeModel, out GameGraphNode runtimeNode)
	{
		if (editorNodeModel is not TModel typedModel)
		{
			runtimeNode = null;
			return false;
		}

		if (!TryConvert(typedModel, out TNode typedRuntimeNode) || typedRuntimeNode == null)
		{
			runtimeNode = null;
			return false;
		}

		runtimeNode = typedRuntimeNode;
		return true;
	}

	protected abstract bool TryConvert(TModel editorNodeModel, out TNode runtimeNode);

	protected static bool TryGetOptionValue<T>(Node node, string optionName, out T value)
	{
		if (node == null)
		{
			value = default;
			return false;
		}

		INodeOption option = node.GetNodeOptionByName(optionName);
		if (option != null && option.TryGetValue(out value))
		{
			return true;
		}

		value = default;
		return false;
	}

	protected static T GetOptionValue<T>(Node node, string optionName, T defaultValue = default)
	{
		return TryGetOptionValue(node, optionName, out T value) ? value : defaultValue;
	}

	protected static string GetStringOption(Node node, string optionName, string defaultValue = null)
	{
		return GetOptionValue(node, optionName, defaultValue);
	}

	protected static int GetIntOption(Node node, string optionName, int defaultValue = 0)
	{
		return GetOptionValue(node, optionName, defaultValue);
	}

	protected static float GetFloatOption(Node node, string optionName, float defaultValue = 0f)
	{
		return GetOptionValue(node, optionName, defaultValue);
	}

	protected static bool GetBoolOption(Node node, string optionName, bool defaultValue = false)
	{
		return GetOptionValue(node, optionName, defaultValue);
	}

	protected static bool TryGetConnectedNodeId(INode node, int outputIndex, IReadOnlyDictionary<INode, string> idMap, out string nodeId)
	{
		nodeId = null;
		if (node == null || idMap == null || outputIndex < 0 || outputIndex >= node.outputPortCount)
		{
			return false;
		}

		IPort outputPort = node.GetOutputPort(outputIndex);
		if (outputPort == null)
		{
			return false;
		}

		IPort nextPort = outputPort.firstConnectedPort;
		INode nextNode = nextPort?.GetNode();
		if (nextNode == null)
		{
			return false;
		}

		return idMap.TryGetValue(nextNode, out nodeId) && !string.IsNullOrWhiteSpace(nodeId);
	}

	protected static string GetConnectedNodeId(INode node, int outputIndex, IReadOnlyDictionary<INode, string> idMap)
	{
		return TryGetConnectedNodeId(node, outputIndex, idMap, out string nodeId) ? nodeId : null;
	}

	protected static bool TryGetConnectedNodeId(INode node, string outputName, IReadOnlyDictionary<INode, string> idMap, out string nodeId)
	{
		nodeId = null;
		if (node == null || idMap == null || string.IsNullOrWhiteSpace(outputName))
		{
			return false;
		}

		for (int i = 0; i < node.outputPortCount; i++)
		{
			IPort outputPort = node.GetOutputPort(i);
			if (outputPort == null)
			{
				continue;
			}

			if (!string.Equals(outputPort.name, outputName, StringComparison.Ordinal))
			{
				continue;
			}

			return TryGetConnectedNodeId(node, i, idMap, out nodeId);
		}

		return false;
	}

	protected static string GetConnectedNodeId(INode node, string outputName, IReadOnlyDictionary<INode, string> idMap)
	{
		return TryGetConnectedNodeId(node, outputName, idMap, out string nodeId) ? nodeId : null;
	}
}
