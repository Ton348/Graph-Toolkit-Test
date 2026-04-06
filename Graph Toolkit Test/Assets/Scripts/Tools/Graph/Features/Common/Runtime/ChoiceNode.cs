using System;
using System.Collections.Generic;

[Serializable]
public class ChoiceNode : BusinessQuestNode
{
    public List<ChoiceOption> options = new List<ChoiceOption>();
}
