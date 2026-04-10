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

		public static CommonGraph BuildBaseGraph(CommonGraphEditorGraph graph)
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

			CommonGraph runtimeGraph = ScriptableObject.CreateInstance<CommonGraph>();
			List<BaseGraphNode> runtimeNodes = new List<BaseGraphNode>(editorNodes.Count);
			Dictionary<INode, BaseGraphNode> nodeMap = new Dictionary<INode, BaseGraphNode>(editorNodes.Count);
			Dictionary<INode, string> idMap = new Dictionary<INode, string>(editorNodes.Count);

			for (int i = 0; i < editorNodes.Count; i++)
			{
				INode editorNode = editorNodes[i];
				BaseGraphNode runtimeNode = ConvertNode(editorNode);
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
				ApplyConnections(pair.Key, pair.Value, idMap);
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

		private static BaseGraphNode ConvertNode(INode node)
		{
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
				MapMarkerNodeModel mapMarkerModel => new MapMarkerNode
				{
					markerId = GetOptionValue<string>(mapMarkerModel, MapMarkerNodeModel.MARKER_ID_OPTION),
					targetObjectName = GetOptionValue<string>(mapMarkerModel, MapMarkerNodeModel.TARGET_OPTION)
				},
				PlayCutsceneNodeModel playCutsceneModel => new PlayCutsceneNode
				{
					cutsceneReference = GetOptionValue<string>(playCutsceneModel, PlayCutsceneNodeModel.CUTSCENE_REFERENCE_OPTION)
				},
				CheckpointNodeModel checkpointModel => new CheckpointNode
				{
					checkpointId = GetOptionValue<string>(checkpointModel, CheckpointNodeModel.CHECKPOINT_ID_OPTION),
					action = GetOptionValue<CheckpointAction>(checkpointModel, CheckpointNodeModel.ACTION_OPTION)
				},
				StartQuestNodeModel startQuestModel => new StartQuestNode
				{
					questId = GetOptionValue<string>(startQuestModel, StartQuestNodeModel.QUEST_ID_OPTION)
				},
				CompleteQuestNodeModel completeQuestModel => new CompleteQuestNode
				{
					questId = GetOptionValue<string>(completeQuestModel, CompleteQuestNodeModel.QUEST_ID_OPTION)
				},
				QuestStateConditionNodeModel questStateModel => new QuestStateConditionNode
				{
					questId = GetOptionValue<string>(questStateModel, QuestStateConditionNodeModel.QUEST_ID_OPTION),
					state = GetOptionValue<QuestState>(questStateModel, QuestStateConditionNodeModel.STATE_OPTION)
				},
				_ => null
			};

			ApplyNodeMetadata(typedNode, runtimeNode);
			return runtimeNode;
		}

		private static ChoiceNode BuildChoiceNode(ChoiceNodeModel model)
		{
			ChoiceNode node = new ChoiceNode();
			string[] labels =
			{
				GetOptionValue<string>(model, ChoiceNodeModel.OPTION1_LABEL),
				GetOptionValue<string>(model, ChoiceNodeModel.OPTION2_LABEL),
				GetOptionValue<string>(model, ChoiceNodeModel.OPTION3_LABEL),
				GetOptionValue<string>(model, ChoiceNodeModel.OPTION4_LABEL)
			};

			for (int i = 0; i < node.options.Count && i < labels.Length; i++)
			{
				if (node.options[i] == null)
				{
					continue;
				}

				node.options[i].label = labels[i];
			}

			return node;
		}

		private static RandomNode BuildRandomNode(RandomNodeModel model)
		{
			RandomNode node = new RandomNode();
			float[] weights =
			{
				GetOptionValue<float>(model, "Weight1"),
				GetOptionValue<float>(model, "Weight2"),
				GetOptionValue<float>(model, "Weight3"),
				GetOptionValue<float>(model, "Weight4")
			};

			for (int i = 0; i < node.options.Count && i < weights.Length; i++)
			{
				if (node.options[i] == null)
				{
					continue;
				}

				node.options[i].weight = Math.Max(0f, weights[i]);
			}

			return node;
		}

		private static void ApplyConnections(INode editorNode, BaseGraphNode runtimeNode, Dictionary<INode, string> idMap)
		{
			if (editorNode == null || runtimeNode == null)
			{
				return;
			}

			switch (runtimeNode)
			{
				case ChoiceNode choiceNode:
					if (choiceNode.options.Count > 0 && choiceNode.options[0] != null)
					{
						choiceNode.options[0].nextNodeId = GetConnectedNodeIdByOutputName(editorNode, ChoiceNodeModel.OPTION1_PORT, idMap);
					}

					if (choiceNode.options.Count > 1 && choiceNode.options[1] != null)
					{
						choiceNode.options[1].nextNodeId = GetConnectedNodeIdByOutputName(editorNode, ChoiceNodeModel.OPTION2_PORT, idMap);
					}

					if (choiceNode.options.Count > 2 && choiceNode.options[2] != null)
					{
						choiceNode.options[2].nextNodeId = GetConnectedNodeIdByOutputName(editorNode, ChoiceNodeModel.OPTION3_PORT, idMap);
					}

					if (choiceNode.options.Count > 3 && choiceNode.options[3] != null)
					{
						choiceNode.options[3].nextNodeId = GetConnectedNodeIdByOutputName(editorNode, ChoiceNodeModel.OPTION4_PORT, idMap);
					}

					return;

				case RandomNode randomNode:
					for (int i = 0; i < randomNode.options.Count; i++)
					{
						if (randomNode.options[i] == null)
						{
							continue;
						}

						randomNode.options[i].nextNodeId = GetConnectedNodeIdByOutputIndex(editorNode, i, idMap);
					}
					return;

				case CheckpointNode checkpointNode:
					checkpointNode.successNodeId = GetConnectedNodeIdByOutputIndex(editorNode, PrimaryOutputIndex, idMap);
					checkpointNode.failNodeId = GetConnectedNodeIdByOutputIndex(editorNode, SecondaryOutputIndex, idMap);
					return;

				case StartQuestNode startQuestNode:
					startQuestNode.successNodeId = GetConnectedNodeIdByOutputIndex(editorNode, PrimaryOutputIndex, idMap);
					startQuestNode.failNodeId = GetConnectedNodeIdByOutputIndex(editorNode, SecondaryOutputIndex, idMap);
					return;

				case CompleteQuestNode completeQuestNode:
					completeQuestNode.successNodeId = GetConnectedNodeIdByOutputIndex(editorNode, PrimaryOutputIndex, idMap);
					completeQuestNode.failNodeId = GetConnectedNodeIdByOutputIndex(editorNode, SecondaryOutputIndex, idMap);
					return;

				case QuestStateConditionNode questStateConditionNode:
					questStateConditionNode.trueNodeId = GetConnectedNodeIdByOutputIndex(editorNode, PrimaryOutputIndex, idMap);
					questStateConditionNode.falseNodeId = GetConnectedNodeIdByOutputIndex(editorNode, SecondaryOutputIndex, idMap);
					return;
			}

			runtimeNode.nextNodeId = GetConnectedNodeIdByOutputIndex(editorNode, PrimaryOutputIndex, idMap);
		}

		private static string GetConnectedNodeIdByOutputIndex(INode node, int outputIndex, Dictionary<INode, string> idMap)
		{
			if (node == null || outputIndex < 0 || outputIndex >= node.outputPortCount)
			{
				return null;
			}

			IPort outputPort = node.GetOutputPort(outputIndex);
			IPort nextPort = outputPort?.firstConnectedPort;
			INode nextNode = nextPort?.GetNode();
			if (nextNode == null)
			{
				return null;
			}

			return idMap.TryGetValue(nextNode, out string id) ? id : null;
		}

		private static string GetConnectedNodeIdByOutputName(INode node, string outputName, Dictionary<INode, string> idMap)
		{
			if (string.IsNullOrWhiteSpace(outputName))
			{
				return null;
			}

			int outputIndex = outputName switch
			{
				ChoiceNodeModel.OPTION1_PORT => 0,
				ChoiceNodeModel.OPTION2_PORT => 1,
				ChoiceNodeModel.OPTION3_PORT => 2,
				ChoiceNodeModel.OPTION4_PORT => 3,
				_ => -1
			};

			return GetConnectedNodeIdByOutputIndex(node, outputIndex, idMap);
		}

		private static void ApplyNodeMetadata(Node editorNode, BaseGraphNode runtimeNode)
		{
			if (editorNode == null || runtimeNode == null)
			{
				return;
			}

			if (TryGetOptionValue(editorNode, CommonGraphEditorNode.TITLE_OPTION, out string title))
			{
				runtimeNode.title = string.IsNullOrWhiteSpace(title) ? runtimeNode.GetType().Name : title;
			}
			else if (string.IsNullOrWhiteSpace(runtimeNode.title))
			{
				runtimeNode.title = runtimeNode.GetType().Name;
			}

			if (TryGetOptionValue(editorNode, CommonGraphEditorNode.DESCRIPTION_OPTION, out string description))
			{
				runtimeNode.description = description;
			}

			if (TryGetOptionValue(editorNode, CommonGraphEditorNode.COMMENT_OPTION, out string comment))
			{
				runtimeNode.comment = comment;
			}
		}

		private static bool TryGetOptionValue<T>(Node node, string optionName, out T value)
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

		private static T GetOptionValue<T>(Node node, string optionName)
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
