using System;

[Serializable]
public class RequestInstallBusinessModuleNode : BaseGraphNode
{
    public string lotId;
    public string moduleId;
    public string successNodeId;
    public string failNodeId;
}
