using System;

[Serializable]
public class CompleteQuestNode : BusinessQuestNode
{
    public string questId;

    public CompleteQuestNode()
    {
        Title = "Завершить квест";
        Description = "Завершает указанный квест.";
    }
}
