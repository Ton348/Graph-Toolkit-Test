using System;
using System.Collections.Generic;

[Serializable]
public class ChoiceNode : BaseGraphNode
{
    public List<ChoiceOption> options = new List<ChoiceOption>();
}
