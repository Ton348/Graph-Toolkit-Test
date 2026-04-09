using System;

namespace GraphCore.BaseNodes.Runtime.UI
{
    [Serializable]
    public sealed class ChoiceOption
    {
        public string label;
        public string nextNodeId;
    }
}
