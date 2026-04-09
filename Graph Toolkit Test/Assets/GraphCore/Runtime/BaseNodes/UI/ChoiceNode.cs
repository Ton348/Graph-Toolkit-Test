using System;
using System.Collections.Generic;

namespace GraphCore.BaseNodes.Runtime.UI
{
    [Serializable]
    public sealed class ChoiceNode : BusinessQuestNode
    {
        public readonly List<ChoiceOption> options = new List<ChoiceOption>(4)
        {
            new ChoiceOption(),
            new ChoiceOption(),
            new ChoiceOption(),
            new ChoiceOption()
        };

        public ChoiceNode()
        {
            Title = "Choice";
            Description = "Shows player choices and branches by selection";
        }
    }
}
