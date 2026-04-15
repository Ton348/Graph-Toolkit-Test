using System;
using Game1.Graph.Runtime;

[Serializable]
public sealed class RequestAssignSupplierNode : GameGraphSuccessFailNode
{
    public string lotId;
    public string supplierId;
}
