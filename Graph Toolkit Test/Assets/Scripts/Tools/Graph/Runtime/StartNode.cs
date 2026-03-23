using System;

[Serializable]
public class StartNode : BusinessQuestNode
{
    public StartNode()
    {
        Title = "Старт";
        Description = "Начальная точка графа.";
    }
}
