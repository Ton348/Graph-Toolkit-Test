public class QuestState
{
    public QuestDefinition Definition;
    public QuestStatus Status;

    public QuestState(QuestDefinition definition)
    {
        Definition = definition;
        Status = QuestStatus.Inactive;
    }
}
