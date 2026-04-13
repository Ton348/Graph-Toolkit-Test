using System;
using System.Collections.Generic;
using GraphCore.BaseNodes.Editor.Flow;
using GraphCore.BaseNodes.Editor.Server;
using GraphCore.BaseNodes.Editor.Cinematics;
using GraphCore.BaseNodes.Editor.UI;
using GraphCore.BaseNodes.Editor.Utility;
using GraphCore.BaseNodes.Editor.World;
using GraphCore.BaseNodes.Runtime.Server;
using GraphCore.BaseNodes.Runtime.Cinematics;
using GraphCore.BaseNodes.Runtime.Flow;
using GraphCore.BaseNodes.Runtime.UI;
using GraphCore.BaseNodes.Runtime.Utility;
using GraphCore.BaseNodes.Runtime.World;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Graph.Core.Editor
{
	public static class CommonGraphImporter
	{
		private const int PrimaryOutputIndex = 0;
		private const int SecondaryOutputIndex = 1;

		public delegate BaseGraphNode NodeConverter(INode editorNode);
		public delegate void ConnectionApplier(INode editorNode, BaseGraphNode runtimeNode, IReadOnlyDictionary<INode, string> idMap);

		private static Func<INode, BaseGraphNode> s_externalConverter;

		public static void SetExternalConverter(Func<INode, BaseGraphNode> converter)
		{
			s_externalConverter = converter;
		}

		public static CommonGraph BuildBaseGraph(CommonGraphEditorGraph graph)
		{
			return BuildBaseGraph(graph, null, null);
		}

		public static CommonGraph BuildBaseGraph(CommonGraphEditorGraph graph, NodeConverter converter, ConnectionApplier connectionApplier)
		{
			if (graph == null)
			{
				return null;
			}

			List<INode> editorNodes = new List<INode>(graph.GetNodes());
			StartNodeModel startNode = FindStartNode(editorNodes);
			if (startNode == null)
			{
				return null;
			}

			NodeConverter effectiveConverter = converter ?? ConvertNode;
			ConnectionApplier effectiveConnectionApplier = connectionApplier ?? ApplyConnections;

			CommonGraph runtimeGraph = ScriptableObject.CreateInstance<CommonGraph>();
			List<BaseGraphNode> runtimeNodes = new List<BaseGraphNode>(editorNodes.Count);
			Dictionary<INode, BaseGraphNode> nodeMap = new Dictionary<INode, BaseGraphNode>(editorNodes.Count);
			Dictionary<INode, string> idMap = new Dictionary<INode, string>(editorNodes.Count);

			for (int i = 0; i < editorNodes.Count; i++)
			{
				INode editorNode = editorNodes[i];
				BaseGraphNode runtimeNode = effectiveConverter(editorNode);
				if (runtimeNode == null)
				{
					continue;
				}

				string nodeId = Guid.NewGuid().ToString();
				runtimeNode.nodeId = nodeId;
				runtimeNodes.Add(runtimeNode);
				nodeMap[editorNode] = runtimeNode;
				idMap[editorNode] = nodeId;
			}

			foreach (KeyValuePair<INode, BaseGraphNode> pair in nodeMap)
			{
				effectiveConnectionApplier(pair.Key, pair.Value, idMap);
			}

			runtimeGraph.startNodeId = idMap.TryGetValue(startNode, out string startId) ? startId : null;
			runtimeGraph.nodes = runtimeNodes;
			return runtimeGraph;
		}

		private static StartNodeModel FindStartNode(IReadOnlyList<INode> editorNodes)
		{
			for (int i = 0; i < editorNodes.Count; i++)
			{
				if (editorNodes[i] is StartNodeModel startNode)
				{
					return startNode;
				}
			}

			return null;
		}

		public static BaseGraphNode ConvertNode(INode node)
		{
			if (node == null)
			{
				return null;
			}

			// 1. try external (Game layer)
			if (s_externalConverter != null)
			{
				BaseGraphNode externalNode = s_externalConverter(node);
				if (externalNode != null)
				{
					if (node is Node extTyped)
					{
						ApplyNodeMetadata(extTyped, externalNode);
					}
					return externalNode;
				}
			}

			// 2. fallback to Core
			if (node is not Node typedNode)
			{
				return null;
			}

			BaseGraphNode runtimeNode = node switch
			{
				StartNodeModel => new StartNode(),
				FinishNodeModel => new FinishNode(),
				DialogueNodeModel dialogueModel => new DialogueNode
				{
					dialogueTitle = GetOptionValue<string>(dialogueModel, "Title"),
					body = GetOptionValue<string>(dialogueModel, "Body")
				},
				ChoiceNodeModel choiceModel => BuildChoiceNode(choiceModel),
				LogNodeModel logModel => new LogNode
				{
					message = GetOptionValue<string>(logModel, "Message")
				},
				DelayNodeModel delayModel => new DelayNode
				{
					delaySeconds = GetOptionValue<float>(delayModel, "DelaySeconds")
				},
				RandomNodeModel randomModel => BuildRandomNode(randomModel),
				_ => null
			};

			ApplyNodeMetadata(typedNode, runtimeNode);
			return runtimeNode;
		}

		private static ChoiceNode BuildChoiceNode(ChoiceNodeModel model)
		{
			ChoiceNode node = new ChoiceNode();
			return node;
		}

		private static RandomNode BuildRandomNode(RandomNodeModel model)
		{
			RandomNode node = new RandomNode();
			return node;
		}

		public static void ApplyConnections(INode editorNode, BaseGraphNode runtimeNode, IReadOnlyDictionary<INode, string> idMap)
		{
			if (editorNode == null || runtimeNode == null)
			{
				return;
			}

			runtimeNode.nextNodeId = GetConnectedNodeIdByOutputIndex(editorNode, PrimaryOutputIndex, idMap);
		}

		public static string GetConnectedNodeIdByOutputIndex(INode node, int outputIndex, IReadOnlyDictionary<INode, string> idMap)
		{
			if (node == null || outputIndex < 0 || outputIndex >= node.outputPortCount)
			{
				return null;
			}

			IPort outputPort = node.GetOutputPort(outputIndex);
			if (outputPort == null)
			{
				return null;
			}

			IPort nextPort = outputPort.firstConnectedPort;
			INode nextNode = nextPort?.GetNode();
			if (nextNode == null)
			{
				return null;
			}

			return idMap.TryGetValue(nextNode, out string id) ? id : null;
		}

		public static void ApplyNodeMetadata(Node editorNode, BaseGraphNode runtimeNode)
		{
			if (editorNode == null || runtimeNode == null)
			{
				return;
			}

			runtimeNode.title = runtimeNode.GetType().Name;
		}

		public static bool TryGetOptionValue<T>(Node node, string optionName, out T value)
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

		public static T GetOptionValue<T>(Node node, string optionName)
		{
			if (node == null)
			{
				return default;
			}

			INodeOption option = node.GetNodeOptionByName(optionName);
			if (option != null && option.TryGetValue(out T value))
			{
				return value;
			}

			return default;
		}
	}
}
