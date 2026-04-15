using System;
using Game1.Graph.Runtime;

[Serializable]
public sealed class RequestAssignBusinessTypeNode : GameGraphSuccessFailNode
{
    public string lotId;
    public string businessTypeId;
}
