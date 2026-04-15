using System;
using Game1.Graph.Runtime;

[Serializable]
public sealed class RequestHireBusinessWorkerNode : GameGraphSuccessFailNode
{
    public string lotId;
    public string roleId;
    public string contactId;
}
