using System;

[Serializable]
public sealed class RequestInstallBusinessModuleNode : GameGraphSuccessFailNode
{
    public string lotId;
    public string moduleId;
}
