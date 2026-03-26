using System.Collections.Generic;

public class QuestService
{
    private readonly GameRuntimeState runtime;
    private readonly EventBus eventBus;

    public QuestService(GameRuntimeState runtime, EventBus eventBus)
    {
        this.runtime = runtime;
        this.eventBus = eventBus;
    }

    public void AcceptQuest(QuestDefinition definition)
    {
        if (runtime == null || runtime.Quests == null || definition == null)
        {
            return;
        }

        if (HasActiveQuest(definition.questId))
        {
            return;
        }

        QuestState quest = new QuestState(definition);
        quest.Status = QuestStatus.Active;
        runtime.Quests.Add(quest);

        eventBus?.Publish(new QuestAcceptedEvent(quest));
    }

    public void CompleteQuest(string questId)
    {
        QuestState quest = GetQuestById(questId);
        if (quest == null)
        {
            return;
        }

        if (quest.Status != QuestStatus.Active)
        {
            return;
        }

        quest.Status = QuestStatus.Completed;
        eventBus?.Publish(new QuestCompletedEvent(quest));
    }

    public void FailQuest(string questId)
    {
        QuestState quest = GetQuestById(questId);
        if (quest == null)
        {
            return;
        }

        if (quest.Status != QuestStatus.Active)
        {
            return;
        }

        quest.Status = QuestStatus.Failed;
    }

    public bool HasActiveQuest(string questId)
    {
        QuestState quest = GetQuestById(questId);
        return quest != null && quest.Status == QuestStatus.Active;
    }

    public QuestState GetQuestById(string questId)
    {
        if (runtime == null || runtime.Quests == null || string.IsNullOrEmpty(questId))
        {
            return null;
        }

        foreach (QuestState quest in runtime.Quests)
        {
            if (quest.Definition != null && quest.Definition.questId == questId)
            {
                return quest;
            }
        }

        return null;
    }
}
