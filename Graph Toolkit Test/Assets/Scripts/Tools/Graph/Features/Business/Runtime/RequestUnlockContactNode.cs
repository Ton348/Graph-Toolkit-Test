using System;

[Serializable]
public class RequestUnlockContactNode : BusinessQuestNode
{
    public string contactId;
    public string successNodeId;
    public string failNodeId;
}
