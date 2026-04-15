using System;
using Game1.Graph.Runtime;

[Serializable]
public sealed class RequestSetBusinessOpenNode : GameGraphSuccessFailNode
{
    public string lotId;
    public bool open;
}
