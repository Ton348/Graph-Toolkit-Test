using System;
using Unity.GraphToolkit.Editor;

public interface IBusinessQuestCommonNode
{
}

[Serializable]
public abstract class BusinessQuestCommonNodeModel : Graph.Core.Editor.GraphEditorNode, IBusinessQuestCommonNode
{
}
