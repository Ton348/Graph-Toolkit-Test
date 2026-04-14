using System;

[Serializable]
public sealed class RequestHireBusinessWorkerNode : GameGraphSuccessFailNode
{
    public string lotId;
    public string roleId;
    public string contactId;
}
