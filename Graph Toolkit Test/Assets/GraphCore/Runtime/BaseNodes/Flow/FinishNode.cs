using System;

namespace GraphCore.BaseNodes.Runtime.Flow
{
    [Serializable]
    public sealed class FinishNode : BusinessQuestNode
    {
        public FinishNode()
        {
            Title = "Finish";
            Description = "Terminates graph execution";
        }
    }
}
