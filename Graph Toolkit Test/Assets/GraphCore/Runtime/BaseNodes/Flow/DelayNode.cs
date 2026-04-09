using System;

namespace GraphCore.BaseNodes.Runtime.Flow
{
    [Serializable]
    public sealed class DelayNode : BusinessQuestNode
    {
        public float delaySeconds = 1f;

        public DelayNode()
        {
            Title = "Delay";
            Description = "Waits for a duration and continues";
        }
    }
}
