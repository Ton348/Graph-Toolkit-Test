using System;
using Game1.Graph.Runtime;

[Serializable]
public sealed class RequestOpenBusinessNode : GameGraphSuccessFailNode
{
    public string lotId;
}
