using System;

[Serializable]
public class EndNode : BusinessQuestNode
{
    public bool clearCheckpoint = true;

    public EndNode()
    {
        Title = "Конец";
        Description = "Завершает выполнение графа.";
    }
}
