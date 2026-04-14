using System;

[Serializable]
public sealed class RequestSetBusinessOpenNode : GameGraphSuccessFailNode
{
    public string lotId;
    public bool open;
}
