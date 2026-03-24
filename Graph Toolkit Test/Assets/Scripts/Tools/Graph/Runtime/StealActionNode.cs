using System;

[Serializable]
public class StealActionNode : BusinessQuestNode
{
    public int stealAmount;
    public bool canFail;
    public int requiredSpeech;
    public string successNodeId;
    public string failNodeId;
}
