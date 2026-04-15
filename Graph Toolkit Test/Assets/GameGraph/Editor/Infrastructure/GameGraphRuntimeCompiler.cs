using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

public static class GameGraphRuntimeCompiler
{
	public static CommonGraph Build(CommonGraphEditorGraph editorGraph)
	{
		return CommonGraphImporter.BuildBaseGraph(editorGraph, CommonGraphImporter.ConvertNode, ApplyConnections);
	}

	private static void ApplyConnections(INode editorNode, BaseGraphNode runtimeNode, System.Collections.Generic.IReadOnlyDictionary<INode, string> idMap)
	{
		CommonGraphImporter.ApplyConnections(editorNode, runtimeNode, idMap);

		if (editorNode is GameGraphSuccessFailNodeModel && runtimeNode is GameGraphSuccessFailNode successFailNode)
		{
			successFailNode.successNodeId = CommonGraphImporter.GetConnectedNodeIdByOutputName(editorNode, GameGraphSuccessFailNodeModel.SUCCESS_PORT, idMap);
			successFailNode.failNodeId = CommonGraphImporter.GetConnectedNodeIdByOutputName(editorNode, GameGraphSuccessFailNodeModel.FAIL_PORT, idMap);
			return;
		}

		if (editorNode is GameGraphTrueFalseNodeModel && runtimeNode is GameGraphTrueFalseNode trueFalseNode)
		{
			trueFalseNode.trueNodeId = CommonGraphImporter.GetConnectedNodeIdByOutputName(editorNode, GameGraphTrueFalseNodeModel.TRUE_PORT, idMap);
			trueFalseNode.falseNodeId = CommonGraphImporter.GetConnectedNodeIdByOutputName(editorNode, GameGraphTrueFalseNodeModel.FALSE_PORT, idMap);
			return;
		}

		if (editorNode is ConditionNodeModel && runtimeNode is ConditionNode conditionNode)
		{
			conditionNode.trueNodeId = CommonGraphImporter.GetConnectedNodeIdByOutputName(editorNode, ConditionNodeModel.TRUE_PORT, idMap);
			conditionNode.falseNodeId = CommonGraphImporter.GetConnectedNodeIdByOutputName(editorNode, ConditionNodeModel.FALSE_PORT, idMap);
		}
	}
}
