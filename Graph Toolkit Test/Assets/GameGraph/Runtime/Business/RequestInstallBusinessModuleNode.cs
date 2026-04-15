using System;
using Game1.Graph.Runtime;

[Serializable]
public sealed class RequestInstallBusinessModuleNode : GameGraphSuccessFailNode
{
    public string lotId;
    public string moduleId;
}
