using System;

[Serializable]
public class RequestSetBusinessOpenNode : BusinessQuestNode
{
    public string lotId;
    public bool open;
    public string successNodeId;
    public string failNodeId;
}
