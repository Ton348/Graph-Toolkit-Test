using System;

[Serializable]
public class SkillCheckNode : BusinessQuestNode
{
    public SkillType skillType;
    public int requiredValue;
    public string successNodeId;
    public string failNodeId;
}
