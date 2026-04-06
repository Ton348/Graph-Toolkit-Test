using System;

[Serializable]
public class RequestHireBusinessWorkerNode : BusinessQuestNode
{
    public string lotId;
    public string roleId;
    public string contactId;
    public string successNodeId;
    public string failNodeId;
}
