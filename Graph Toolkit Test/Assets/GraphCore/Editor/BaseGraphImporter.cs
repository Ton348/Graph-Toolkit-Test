using System;
using System.Collections.Generic;
using System.Linq;
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
    public static class BaseGraphImporter
    {
        public static BaseGraph BuildBaseGraph(BaseGraphEditorGraph graph)
        {
            if (graph == null)
            {
                return null;
            }

            StartNodeModel startNode = graph.GetNodes().OfType<StartNodeModel>().FirstOrDefault();
            if (startNode == null)
            {
                return null;
            }

            var runtimeGraph = ScriptableObject.CreateInstance<BaseGraph>();
            var runtimeNodes = new List<BusinessQuestNode>();
            var nodeMap = new Dictionary<INode, BusinessQuestNode>();
            var idMap = new Dictionary<INode, string>();

            foreach (INode node in graph.GetNodes())
            {
                BusinessQuestNode runtimeNode = ConvertNode(node);
                if (runtimeNode == null)
                {
                    continue;
                }

                runtimeNode.id = Guid.NewGuid().ToString();
                runtimeNodes.Add(runtimeNode);
                nodeMap[node] = runtimeNode;
                idMap[node] = runtimeNode.id;
            }

            foreach ((INode node, BusinessQuestNode runtimeNode) in nodeMap)
            {
                ApplyConnections(node, runtimeNode, idMap);
            }

            runtimeGraph.startNodeId = idMap.TryGetValue(startNode, out string startId) ? startId : null;
            runtimeGraph.nodes = runtimeNodes;
            return runtimeGraph;
        }

        static BusinessQuestNode ConvertNode(INode node)
        {
            if (node is not Node typedNode)
            {
                return null;
            }

            BusinessQuestNode runtimeNode = node switch
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

        static ChoiceNode BuildChoiceNode(ChoiceNodeModel model)
        {
            var node = new ChoiceNode();
            string[] labels =
            {
                GetOptionValue<string>(model, "Option1"),
                GetOptionValue<string>(model, "Option2"),
                GetOptionValue<string>(model, "Option3"),
                GetOptionValue<string>(model, "Option4")
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

        static RandomNode BuildRandomNode(RandomNodeModel model)
        {
            var node = new RandomNode();
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

        static void ApplyConnections(INode editorNode, BusinessQuestNode runtimeNode, Dictionary<INode, string> idMap)
        {
            if (editorNode == null || runtimeNode == null)
            {
                return;
            }

            switch (runtimeNode)
            {
                case ChoiceNode choiceNode:
                    for (int i = 0; i < choiceNode.options.Count; i++)
                    {
                        if (choiceNode.options[i] == null)
                        {
                            continue;
                        }

                        choiceNode.options[i].nextNodeId = GetConnectedNodeIdByOutputIndex(editorNode, i, idMap);
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
                    checkpointNode.successNodeId = GetConnectedNodeIdByOutputIndex(editorNode, 0, idMap);
                    checkpointNode.failNodeId = GetConnectedNodeIdByOutputIndex(editorNode, 1, idMap);
                    return;

                case StartQuestNode startQuestNode:
                    startQuestNode.successNodeId = GetConnectedNodeIdByOutputIndex(editorNode, 0, idMap);
                    startQuestNode.failNodeId = GetConnectedNodeIdByOutputIndex(editorNode, 1, idMap);
                    return;

                case CompleteQuestNode completeQuestNode:
                    completeQuestNode.successNodeId = GetConnectedNodeIdByOutputIndex(editorNode, 0, idMap);
                    completeQuestNode.failNodeId = GetConnectedNodeIdByOutputIndex(editorNode, 1, idMap);
                    return;

                case QuestStateConditionNode questStateConditionNode:
                    questStateConditionNode.trueNodeId = GetConnectedNodeIdByOutputIndex(editorNode, 0, idMap);
                    questStateConditionNode.falseNodeId = GetConnectedNodeIdByOutputIndex(editorNode, 1, idMap);
                    return;
            }

            runtimeNode.nextNodeId = GetConnectedNodeIdByOutputIndex(editorNode, 0, idMap);
        }

        static string GetConnectedNodeIdByOutputIndex(INode node, int outputIndex, Dictionary<INode, string> idMap)
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

        static void ApplyNodeMetadata(Node editorNode, BusinessQuestNode runtimeNode)
        {
            if (editorNode == null || runtimeNode == null)
            {
                return;
            }

            if (TryGetOptionValue(editorNode, BusinessQuestEditorNode.TITLE_OPTION, out string title))
            {
                runtimeNode.Title = string.IsNullOrWhiteSpace(title) ? runtimeNode.GetType().Name : title;
            }
            else if (string.IsNullOrWhiteSpace(runtimeNode.Title))
            {
                runtimeNode.Title = runtimeNode.GetType().Name;
            }

            if (TryGetOptionValue(editorNode, BusinessQuestEditorNode.DESCRIPTION_OPTION, out string description))
            {
                runtimeNode.Description = description;
            }

            if (TryGetOptionValue(editorNode, BusinessQuestEditorNode.COMMENT_OPTION, out string comment))
            {
                runtimeNode.Comment = comment;
            }
        }

        static bool TryGetOptionValue<T>(Node node, string optionName, out T value)
        {
            INodeOption option = node.GetNodeOptionByName(optionName);
            if (option != null && option.TryGetValue(out value))
            {
                return true;
            }

            value = default;
            return false;
        }

        static T GetOptionValue<T>(Node node, string optionName)
        {
            INodeOption option = node.GetNodeOptionByName(optionName);
            if (option != null && option.TryGetValue(out T value))
            {
                return value;
            }

            return default;
        }
    }
}
