using System;

namespace GraphCore.BaseNodes.Runtime.Flow
{
    [Serializable]
    public sealed class StartNode : BusinessQuestNode
    {
        public StartNode()
        {
            Title = "Start";
            Description = "Entry point of the graph";
        }
    }
}
