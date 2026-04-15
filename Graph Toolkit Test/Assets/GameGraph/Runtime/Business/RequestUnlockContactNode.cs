using System;
using Game1.Graph.Runtime;

[Serializable]
public sealed class RequestUnlockContactNode : GameGraphSuccessFailNode
{
    public string contactId;
}
