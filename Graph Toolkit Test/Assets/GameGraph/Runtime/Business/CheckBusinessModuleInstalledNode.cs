using System;
using Game1.Graph.Runtime;

[Serializable]
public sealed class CheckBusinessModuleInstalledNode : GameGraphTrueFalseNode
{
    public string lotId;
    public string moduleId;
}
