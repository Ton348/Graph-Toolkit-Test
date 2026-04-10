using System;

[Serializable]
public class RequestAssignBusinessTypeNode : BaseGraphNode
{
    public string lotId;
    public string businessTypeId;
    public string successNodeId;
    public string failNodeId;
}
