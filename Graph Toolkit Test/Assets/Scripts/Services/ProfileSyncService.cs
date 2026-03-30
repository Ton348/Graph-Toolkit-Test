using System;
using System.Collections.Generic;

public class ProfileSyncService
{
    private readonly GameRuntimeState runtime;
    private readonly List<QuestDefinition> questDefinitions;
    private readonly List<BuildingDefinition> buildingDefinitions;

    public event Action<ProfileSnapshot> Synced;

    public ProfileSyncService(GameRuntimeState runtime, List<QuestDefinition> questDefinitions, List<BuildingDefinition> buildingDefinitions)
    {
        this.runtime = runtime;
        this.questDefinitions = questDefinitions;
        this.buildingDefinitions = buildingDefinitions;
    }

    public void ApplySnapshot(ProfileSnapshot snapshot)
    {
        if (snapshot == null || runtime == null)
        {
            return;
        }

        if (runtime.Player != null)
        {
            runtime.Player.Money = snapshot.Money;
        }

        ApplyQuests(snapshot);
        ApplyBuildings(snapshot);

        Synced?.Invoke(snapshot);
    }

    private void ApplyQuests(ProfileSnapshot snapshot)
    {
        if (runtime.Quests == null || questDefinitions == null)
        {
            return;
        }

        var activeSet = new HashSet<string>(snapshot.ActiveQuestIds ?? new List<string>());
        var completedSet = new HashSet<string>(snapshot.CompletedQuestIds ?? new List<string>());

        foreach (QuestState quest in runtime.Quests)
        {
            if (quest == null || quest.Definition == null)
            {
                continue;
            }

            string id = quest.Definition.questId;
            if (completedSet.Contains(id))
            {
                quest.Status = QuestStatus.Completed;
            }
            else if (activeSet.Contains(id))
            {
                quest.Status = QuestStatus.Active;
            }
            else
            {
                if (quest.Status != QuestStatus.Failed)
                {
                    quest.Status = QuestStatus.Inactive;
                }
            }
        }

        EnsureQuestStates(activeSet, QuestStatus.Active);
        EnsureQuestStates(completedSet, QuestStatus.Completed);
    }

    private void EnsureQuestStates(HashSet<string> ids, QuestStatus status)
    {
        if (ids == null || questDefinitions == null || runtime.Quests == null)
        {
            return;
        }

        foreach (string id in ids)
        {
            if (string.IsNullOrEmpty(id))
            {
                continue;
            }

            QuestState existing = FindQuestState(id, runtime.Quests);
            if (existing != null)
            {
                existing.Status = status;
                continue;
            }

            QuestDefinition def = FindQuestDefinition(id, questDefinitions);
            if (def == null)
            {
                continue;
            }

            QuestState state = new QuestState(def)
            {
                Status = status
            };
            runtime.Quests.Add(state);
        }
    }

    private void ApplyBuildings(ProfileSnapshot snapshot)
    {
        if (runtime.Buildings == null)
        {
            return;
        }

        var ownedSet = new HashSet<string>(snapshot.OwnedBuildingIds ?? new List<string>());

        foreach (BuildingState building in runtime.Buildings)
        {
            if (building == null || building.Definition == null)
            {
                continue;
            }

            string id = building.Definition.buildingId;
            building.IsOwned = ownedSet.Contains(id);
        }
    }

    private static QuestState FindQuestState(string questId, List<QuestState> quests)
    {
        if (quests == null) return null;

        foreach (QuestState quest in quests)
        {
            if (quest != null && quest.Definition != null && quest.Definition.questId == questId)
            {
                return quest;
            }
        }

        return null;
    }

    private static QuestDefinition FindQuestDefinition(string questId, List<QuestDefinition> quests)
    {
        if (quests == null) return null;

        foreach (QuestDefinition def in quests)
        {
            if (def != null && def.questId == questId)
            {
                return def;
            }
        }

        return null;
    }
}
