using System;

namespace GraphCore.BaseNodes.Runtime.Server
{
    [Serializable]
    public sealed class CheckpointNode : BusinessQuestNode
    {
        public string checkpointId;
        public CheckpointAction action;
        public string successNodeId;
        public string failNodeId;

        public CheckpointNode()
        {
            Title = "Checkpoint";
            Description = "Saves or clears checkpoint state";
        }
    }
}
