using System;
using Game1.Graph.Runtime;

using Game1.Graph.Runtime.Templates;
[Serializable]
public sealed class RequestHireBusinessWorkerNode : GameGraphSuccessFailNode
{
    public string lotId;
    public string roleId;
    public string contactId;
}
