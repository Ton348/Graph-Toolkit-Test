using System;

[Serializable]
public sealed class RequestAssignSupplierNode : GameGraphSuccessFailNode
{
    public string lotId;
    public string supplierId;
}
