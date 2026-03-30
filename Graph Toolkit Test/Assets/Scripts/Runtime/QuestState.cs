public class QuestState
{
    public QuestDefinitionData Definition;
    public QuestStatus Status;

    public QuestState(QuestDefinitionData definition)
    {
        Definition = definition;
        Status = QuestStatus.Inactive;
    }
}
