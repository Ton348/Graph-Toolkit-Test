using System;

[Serializable]
public class RequestAssignBusinessTypeNode : BusinessQuestNode
{
    public string lotId;
    public string businessTypeId;
    public string successNodeId;
    public string failNodeId;
}
