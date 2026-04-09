using System;

namespace GraphCore.BaseNodes.Runtime.Utility
{
    [Serializable]
    public sealed class LogNode : BusinessQuestNode
    {
        public string message;

        public LogNode()
        {
            Title = "Log";
            Description = "Writes a debug message";
        }
    }
}
