using System;
using Unity.GraphToolkit.Editor;

public interface IBusinessQuestQuestNode
{
}

[Serializable]
public abstract class BusinessQuestQuestNodeModel : Graph.Core.Editor.GraphEditorNode, IBusinessQuestQuestNode
{
}
