using GraphCore.BaseNodes.Editor.Cinematics;
using GraphCore.BaseNodes.Editor.Flow;
using GraphCore.BaseNodes.Editor.Server;
using GraphCore.BaseNodes.Editor.UI;
using GraphCore.Runtime;
using GraphCore.Runtime.Nodes.Cinematics;
using GraphCore.Runtime.Nodes.Flow;
using GraphCore.Runtime.Nodes.Server;
using GraphCore.Runtime.Nodes.UI;
using GraphCore.Runtime.Nodes.Utility;
using GraphCore.Runtime.Nodes.World;
using GraphCore.Runtime.Templates;
using System;
using System.Collections.Generic;
using System.Reflection;
using GraphCore.BaseNodes.Editor.Utility;
using GraphCore.Editor.BaseNodes.Flow;
using GraphCore.Editor.BaseNodes.Server;
using GraphCore.Editor.BaseNodes.UI;
using GraphCore.Editor.BaseNodes.World;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace GraphCore.Editor
{
	public static class CommonGraphImporter
	{
		private const int s_primaryOutputIndex = 0;
		private const int s_secondaryOutputIndex = 1;

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
				CheckpointNodeModel checkpointModel => new CheckpointNode
				{
					checkpointId = GetOptionValue<string>(checkpointModel, CheckpointNodeModel.CheckpointIdOption),
					action = GetOptionValue<CheckpointAction>(checkpointModel, CheckpointNodeModel.ActionOption)
				},
				StartQuestNodeModel startQuestModel => new StartQuestNode
				{
					questId = GetOptionValue<string>(startQuestModel, StartQuestNodeModel.QuestIdOption)
				},
				CompleteQuestNodeModel completeQuestModel => new CompleteQuestNode
				{
					questId = GetOptionValue<string>(completeQuestModel, CompleteQuestNodeModel.QuestIdOption)
				},
				QuestStateConditionNodeModel questStateConditionModel => new QuestStateConditionNode
				{
					questId = GetOptionValue<string>(questStateConditionModel, QuestStateConditionNodeModel.QuestIdOption),
					state = GetOptionValue<QuestState>(questStateConditionModel, QuestStateConditionNodeModel.StateOption)
				},
				MapMarkerNodeModel mapMarkerModel => new MapMarkerNode
				{
					markerId = GetOptionValue<string>(mapMarkerModel, MapMarkerNodeModel.MarkerIdOption),
					targetObjectName = GetOptionValue<string>(mapMarkerModel, MapMarkerNodeModel.TargetOption)
				},
				PlayCutsceneNodeModel playCutsceneModel => new PlayCutsceneNode
				{
					cutsceneReference = GetOptionValue<string>(playCutsceneModel, PlayCutsceneNodeModel.CutsceneReferenceOption)
				},
				_ => null
			};

			ApplyNodeMetadata(typedNode, runtimeNode);
			return runtimeNode;
		}

		private static ChoiceNode BuildChoiceNode(ChoiceNodeModel model)
		{
			ChoiceNode node = new ChoiceNode();
			node.options[0].label = GetOptionValue<string>(model, ChoiceNodeModel.Option1Label);
			node.options[1].label = GetOptionValue<string>(model, ChoiceNodeModel.Option2Label);
			node.options[2].label = GetOptionValue<string>(model, ChoiceNodeModel.Option3Label);
			node.options[3].label = GetOptionValue<string>(model, ChoiceNodeModel.Option4Label);
			return node;
		}

		private static RandomNode BuildRandomNode(RandomNodeModel model)
		{
			RandomNode node = new RandomNode();
			node.options[0].weight = GetOptionValue<float>(model, RandomNodeModel.Weight1Option);
			node.options[1].weight = GetOptionValue<float>(model, RandomNodeModel.Weight2Option);
			node.options[2].weight = GetOptionValue<float>(model, RandomNodeModel.Weight3Option);
			node.options[3].weight = GetOptionValue<float>(model, RandomNodeModel.Weight4Option);
			return node;
		}

		public static void ApplyConnections(INode editorNode, BaseGraphNode runtimeNode, IReadOnlyDictionary<INode, string> idMap)
		{
			if (editorNode == null || runtimeNode == null)
			{
				return;
			}

			if (editorNode is ChoiceNodeModel && runtimeNode is ChoiceNode choiceNode)
			{
				choiceNode.options[0].nextNodeId = GetConnectedNodeIdByOutputName(editorNode, ChoiceNodeModel.Option1Port, idMap);
				choiceNode.options[1].nextNodeId = GetConnectedNodeIdByOutputName(editorNode, ChoiceNodeModel.Option2Port, idMap);
				choiceNode.options[2].nextNodeId = GetConnectedNodeIdByOutputName(editorNode, ChoiceNodeModel.Option3Port, idMap);
				choiceNode.options[3].nextNodeId = GetConnectedNodeIdByOutputName(editorNode, ChoiceNodeModel.Option4Port, idMap);
				return;
			}

			if (editorNode is RandomNodeModel && runtimeNode is RandomNode randomNode)
			{
				randomNode.options[0].nextNodeId = GetConnectedNodeIdByOutputName(editorNode, RandomNodeModel.Option1Port, idMap);
				randomNode.options[1].nextNodeId = GetConnectedNodeIdByOutputName(editorNode, RandomNodeModel.Option2Port, idMap);
				randomNode.options[2].nextNodeId = GetConnectedNodeIdByOutputName(editorNode, RandomNodeModel.Option3Port, idMap);
				randomNode.options[3].nextNodeId = GetConnectedNodeIdByOutputName(editorNode, RandomNodeModel.Option4Port, idMap);
				return;
			}

			if (editorNode is CheckpointNodeModel && runtimeNode is CheckpointNode checkpointNode)
			{
				checkpointNode.successNodeId = GetConnectedNodeIdByOutputName(editorNode, CheckpointNodeModel.SuccessPort, idMap);
				checkpointNode.failNodeId = GetConnectedNodeIdByOutputName(editorNode, CheckpointNodeModel.FailPort, idMap);
				return;
			}

			if (editorNode is StartQuestNodeModel && runtimeNode is StartQuestNode startQuestNode)
			{
				startQuestNode.successNodeId = GetConnectedNodeIdByOutputName(editorNode, StartQuestNodeModel.SuccessPort, idMap);
				startQuestNode.failNodeId = GetConnectedNodeIdByOutputName(editorNode, StartQuestNodeModel.FailPort, idMap);
				return;
			}

			if (editorNode is CompleteQuestNodeModel && runtimeNode is CompleteQuestNode completeQuestNode)
			{
				completeQuestNode.successNodeId = GetConnectedNodeIdByOutputName(editorNode, CompleteQuestNodeModel.SuccessPort, idMap);
				completeQuestNode.failNodeId = GetConnectedNodeIdByOutputName(editorNode, CompleteQuestNodeModel.FailPort, idMap);
				return;
			}

			if (editorNode is QuestStateConditionNodeModel && runtimeNode is QuestStateConditionNode questStateConditionNode)
			{
				questStateConditionNode.trueNodeId = GetConnectedNodeIdByOutputName(editorNode, QuestStateConditionNodeModel.TruePort, idMap);
				questStateConditionNode.falseNodeId = GetConnectedNodeIdByOutputName(editorNode, QuestStateConditionNodeModel.FalsePort, idMap);
				return;
			}

			if (runtimeNode is CoreGraphNextNode coreGraphNextNode)
			{
				coreGraphNextNode.nextNodeId = GetConnectedNodeIdByOutputIndex(editorNode, s_primaryOutputIndex, idMap);
				return;
			}

			TrySetNextNodeId(runtimeNode, GetConnectedNodeIdByOutputIndex(editorNode, s_primaryOutputIndex, idMap));
		}

		private static void TrySetNextNodeId(BaseGraphNode node, string nextNodeId)
		{
			if (node == null)
			{
				return;
			}

			FieldInfo field = node.GetType().GetField("nextNodeId", BindingFlags.Public | BindingFlags.Instance);
			if (field != null && field.FieldType == typeof(string))
			{
				field.SetValue(node, nextNodeId);
			}
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

		public static string GetConnectedNodeIdByOutputName(INode node, string outputPortName, IReadOnlyDictionary<INode, string> idMap)
		{
			if (node == null || string.IsNullOrWhiteSpace(outputPortName))
			{
				return null;
			}

			IPort outputPort = node.GetOutputPortByName(outputPortName);
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