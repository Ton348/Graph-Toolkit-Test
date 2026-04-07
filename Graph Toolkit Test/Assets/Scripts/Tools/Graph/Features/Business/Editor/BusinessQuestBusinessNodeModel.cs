using System;
using Unity.GraphToolkit.Editor;

public interface IBusinessQuestBusinessNode
{
}

public interface IBusinessQuestLegacyNode
{
}

[Serializable]
public abstract class BusinessQuestBusinessNodeModel : Graph.Core.Editor.GraphEditorNode, IBusinessQuestBusinessNode
{
}

[Serializable]
public abstract class BusinessQuestLegacyBusinessNodeModel : BusinessQuestBusinessNodeModel, IBusinessQuestLegacyNode
{
}
