using System;

[Serializable]
public sealed class RequestSetBusinessMarkupNode : GameGraphSuccessFailNode
{
    public string lotId;
    public int markupPercent;
}
