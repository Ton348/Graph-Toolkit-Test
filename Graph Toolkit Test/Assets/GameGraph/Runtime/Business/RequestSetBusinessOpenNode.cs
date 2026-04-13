using System;

[Serializable]
public class RequestSetBusinessOpenNode : BaseGraphNode
{
    public string lotId;
    public bool open;
    public string successNodeId;
    public string failNodeId;
}
