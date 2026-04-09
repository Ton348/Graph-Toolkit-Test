using System;

[Serializable]
public class EndNode : BaseGraphNode
{
    public bool clearCheckpoint = true;
    public string completeQuestId;

    public EndNode()
    {
        Title = "Конец";
        Description = "Завершает выполнение графа.";
    }
}
