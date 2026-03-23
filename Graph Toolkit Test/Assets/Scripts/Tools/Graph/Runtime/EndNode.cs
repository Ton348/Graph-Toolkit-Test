using System;

[Serializable]
public class EndNode : BusinessQuestNode
{
    public EndNode()
    {
        Title = "Конец";
        Description = "Завершает выполнение графа.";
    }
}
