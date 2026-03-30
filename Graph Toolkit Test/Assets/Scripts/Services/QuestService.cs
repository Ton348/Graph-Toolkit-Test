using System.Collections.Generic;

[System.Obsolete("Legacy client runtime. Only LocalGameServer uses this service.")]
public class QuestService
{
    private readonly GameRuntimeState runtime;

    public QuestService(GameRuntimeState runtime)
    {
        this.runtime = runtime;
    }

    public void AcceptQuest(QuestDefinitionData definition)
    {
        if (runtime == null || runtime.Quests == null || definition == null)
        {
            return;
        }

        if (HasActiveQuest(definition.id))
        {
            return;
        }

        QuestState quest = new QuestState(definition);
        quest.Status = QuestStatus.Active;
        runtime.Quests.Add(quest);

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
            if (quest.Definition != null && quest.Definition.id == questId)
            {
                return quest;
            }
        }

        return null;
    }
}
