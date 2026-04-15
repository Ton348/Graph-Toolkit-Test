using System;
using Game1.Graph.Runtime;

using Game1.Graph.Runtime.Templates;
[Serializable]
public sealed class RequestInstallBusinessModuleNode : GameGraphSuccessFailNode
{
    public string lotId;
    public string moduleId;
}
