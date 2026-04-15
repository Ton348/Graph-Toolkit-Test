using System;
using Game1.Graph.Runtime;

using Game1.Graph.Runtime.Templates;
[Serializable]
public sealed class RequestAssignBusinessTypeNode : GameGraphSuccessFailNode
{
    public string lotId;
    public string businessTypeId;
}
