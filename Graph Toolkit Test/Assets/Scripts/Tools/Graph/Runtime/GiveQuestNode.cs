using System;

[Serializable]
public class GiveQuestNode : BusinessQuestNode
{
    public QuestDefinition questDefinition;

    public GiveQuestNode()
    {
        Title = "Выдать квест";
        Description = "Выдает игроку указанный квест.";
    }
}
