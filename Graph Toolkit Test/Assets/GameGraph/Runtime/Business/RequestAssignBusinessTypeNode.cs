using System;

[Serializable]
public sealed class RequestAssignBusinessTypeNode : GameGraphSuccessFailNode
{
    public string lotId;
    public string businessTypeId;
}
