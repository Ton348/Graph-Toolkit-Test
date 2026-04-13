using System;

[Serializable]
public class RequestHireBusinessWorkerNode : BaseGraphNode
{
    public string lotId;
    public string roleId;
    public string contactId;
    public string successNodeId;
    public string failNodeId;
}
