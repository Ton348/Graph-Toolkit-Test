using System;
using Unity.GraphToolkit.Editor;

public interface IBusinessQuestCommonNode
{
}

[UseWithGraph(typeof(Graph.Core.Editor.GraphEditorGraph))]
[Serializable]
public abstract class BusinessQuestCommonNodeModel : Graph.Core.Editor.GraphEditorNode, IBusinessQuestCommonNode
{
}
