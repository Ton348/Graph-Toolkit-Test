using System;
using Unity.GraphToolkit.Editor;

public interface IBusinessQuestBusinessNode
{
}

public interface IBusinessQuestLegacyNode
{
}

[Serializable]
public abstract class BusinessQuestBusinessNodeModel : BusinessQuestEditorNode, IBusinessQuestBusinessNode
{
}

[Serializable]
public abstract class BusinessQuestLegacyBusinessNodeModel : BusinessQuestBusinessNodeModel, IBusinessQuestLegacyNode
{
}
