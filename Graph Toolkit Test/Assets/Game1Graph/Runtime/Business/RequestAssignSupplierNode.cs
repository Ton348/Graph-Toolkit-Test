using System;

[Serializable]
public class RequestAssignSupplierNode : BusinessQuestNode
{
    public string lotId;
    public string supplierId;
    public string successNodeId;
    public string failNodeId;
}
