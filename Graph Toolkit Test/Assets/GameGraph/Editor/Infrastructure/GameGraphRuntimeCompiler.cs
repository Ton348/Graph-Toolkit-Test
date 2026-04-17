using System.Collections.Generic;
using Game1.Graph.Runtime.Templates;
using GameGraph.Editor.Business;
using GameGraph.Editor.Common;
using GameGraph.Runtime.Common;
using Graph.Core.Editor;
using Graph.Core.Runtime;
using Unity.GraphToolkit.Editor;

namespace GameGraph.Editor.Infrastructure
{
	public static class GameGraphRuntimeCompiler
	{
		public static CommonGraph Build(CommonGraphEditorGraph editorGraph)
		{
			return CommonGraphImporter.BuildBaseGraph(editorGraph, CommonGraphImporter.ConvertNode, ApplyConnections);
		}

		private static void ApplyConnections(
			INode editorNode,
			BaseGraphNode runtimeNode,
			IReadOnlyDictionary<INode, string> idMap)
		{
			CommonGraphImporter.ApplyConnections(editorNode, runtimeNode, idMap);

			if (editorNode is GameGraphSuccessFailNodeModel && runtimeNode is GameGraphSuccessFailNode successFailNode)
			{
				successFailNode.successNodeId =
					CommonGraphImporter.GetConnectedNodeIdByOutputName(editorNode,
						GameGraphSuccessFailNodeModel.SuccessPort, idMap);
				successFailNode.failNodeId =
					CommonGraphImporter.GetConnectedNodeIdByOutputName(editorNode, GameGraphSuccessFailNodeModel.FailPort,
						idMap);
				return;
			}

			if (editorNode is GameGraphTrueFalseNodeModel && runtimeNode is GameGraphTrueFalseNode trueFalseNode)
			{
				trueFalseNode.trueNodeId =
					CommonGraphImporter.GetConnectedNodeIdByOutputName(editorNode, GameGraphTrueFalseNodeModel.TruePort,
						idMap);
				trueFalseNode.falseNodeId =
					CommonGraphImporter.GetConnectedNodeIdByOutputName(editorNode, GameGraphTrueFalseNodeModel.FalsePort,
						idMap);
				return;
			}

			if (editorNode is ConditionNodeModel && runtimeNode is ConditionNode conditionNode)
			{
				conditionNode.trueNodeId =
					CommonGraphImporter.GetConnectedNodeIdByOutputName(editorNode, ConditionNodeModel.TruePort, idMap);
				conditionNode.falseNodeId =
					CommonGraphImporter.GetConnectedNodeIdByOutputName(editorNode, ConditionNodeModel.FalsePort, idMap);
			}
		}
	}
}