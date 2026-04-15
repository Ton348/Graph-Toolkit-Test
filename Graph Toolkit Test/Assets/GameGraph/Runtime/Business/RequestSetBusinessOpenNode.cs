using System;
using Game1.Graph.Runtime;

using Game1.Graph.Runtime.Templates;
[Serializable]
public sealed class RequestSetBusinessOpenNode : GameGraphSuccessFailNode
{
    public string lotId;
    public bool open;
}
