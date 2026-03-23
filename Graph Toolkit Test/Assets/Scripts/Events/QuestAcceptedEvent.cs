public class QuestAcceptedEvent
{
    public QuestState Quest;

    public QuestAcceptedEvent(QuestState quest)
    {
        Quest = quest;
    }
}
