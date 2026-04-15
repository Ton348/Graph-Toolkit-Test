using System;
using Game1.Graph.Runtime;

using Game1.Graph.Runtime.Templates;
[Serializable]
public sealed class CheckBusinessModuleInstalledNode : GameGraphTrueFalseNode
{
    public string lotId;
    public string moduleId;
}
