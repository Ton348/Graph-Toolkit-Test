using System;

[Serializable]
public class StartNode : BaseGraphNode
{
    public StartNode()
    {
        Title = "Старт";
        Description = "Начальная точка графа.";
    }
}
