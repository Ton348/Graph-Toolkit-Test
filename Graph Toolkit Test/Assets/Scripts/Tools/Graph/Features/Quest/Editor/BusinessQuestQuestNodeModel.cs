using System;
using Unity.GraphToolkit.Editor;

public interface IBusinessQuestQuestNode
{
}

[UseWithGraph(typeof(Graph.Core.Editor.GraphEditorGraph))]
[Serializable]
public abstract class BusinessQuestQuestNodeModel : Graph.Core.Editor.GraphEditorNode, IBusinessQuestQuestNode
{
}
