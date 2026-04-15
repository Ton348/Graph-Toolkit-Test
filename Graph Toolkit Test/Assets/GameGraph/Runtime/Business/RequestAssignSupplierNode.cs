using System;
using Game1.Graph.Runtime;

using Game1.Graph.Runtime.Templates;
[Serializable]
public sealed class RequestAssignSupplierNode : GameGraphSuccessFailNode
{
    public string lotId;
    public string supplierId;
}
