using System;
using Game1.Graph.Runtime;

using Game1.Graph.Runtime.Templates;
[Serializable]
public sealed class RequestOpenBusinessNode : GameGraphSuccessFailNode
{
    public string lotId;
}
