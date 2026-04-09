using System;

[Serializable]
public class CheckBusinessModuleInstalledNode : BusinessQuestNode
{
    public string lotId;
    public string moduleId;
    public string trueNodeId;
    public string falseNodeId;
}
