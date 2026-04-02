using System;

[Serializable]
public class RequestInstallBusinessModuleNode : BusinessQuestNode
{
    public string lotId;
    public string moduleId;
    public string successNodeId;
    public string failNodeId;
}
