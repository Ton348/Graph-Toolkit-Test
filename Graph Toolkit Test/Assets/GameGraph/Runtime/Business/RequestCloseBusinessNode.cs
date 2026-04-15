using System;
using Game1.Graph.Runtime;

using Game1.Graph.Runtime.Templates;
[Serializable]
public sealed class RequestCloseBusinessNode : GameGraphSuccessFailNode
{
    public string lotId;
}
