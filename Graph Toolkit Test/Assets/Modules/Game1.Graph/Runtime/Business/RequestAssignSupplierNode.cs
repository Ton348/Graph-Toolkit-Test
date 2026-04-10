using System;

[Serializable]
public class RequestAssignSupplierNode : BaseGraphNode
{
    public string lotId;
    public string supplierId;
    public string successNodeId;
    public string failNodeId;
}
