using System;
using Game1.Graph.Runtime;

[Serializable]
public sealed class RequestCloseBusinessNode : GameGraphSuccessFailNode
{
    public string lotId;
}
