using System;

[Serializable]
public class RequestUnlockContactNode : BaseGraphNode
{
    public string contactId;
    public string successNodeId;
    public string failNodeId;
}
