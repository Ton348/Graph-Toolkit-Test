using System;

[Serializable]
public class RequestSetBusinessMarkupNode : BusinessQuestNode
{
    public string lotId;
    public int markupPercent;
    public string successNodeId;
    public string failNodeId;
}
