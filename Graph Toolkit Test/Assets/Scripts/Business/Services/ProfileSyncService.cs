using System;
using System.Collections.Generic;
using UnityEngine;

public class ProfileSyncService
{
    private readonly GameRuntimeState runtime;
    private readonly GameDataRepository dataRepository;
    private readonly PlayerStateSync playerStateSync;
    private readonly BusinessStateSyncService businessStateSync;

    public event Action<ProfileSnapshot> Synced;

    public ProfileSyncService(GameRuntimeState runtime, GameDataRepository dataRepository, PlayerStateSync playerStateSync, BusinessStateSyncService businessStateSync)
    {
        this.runtime = runtime;
        this.dataRepository = dataRepository;
        this.playerStateSync = playerStateSync;
        this.businessStateSync = businessStateSync;
    }

    public void ApplySnapshot(ProfileSnapshot snapshot)
    {
        if (snapshot == null)
        {
            return;
        }

        playerStateSync?.ApplySnapshot(snapshot);
        businessStateSync?.ApplySnapshot(snapshot);

        Debug.Log($"[ProfileSync] Applied snapshot: money={snapshot.Money}, active={snapshot.ActiveQuestIds?.Count ?? 0}, completed={snapshot.CompletedQuestIds?.Count ?? 0}, owned={snapshot.OwnedBuildingIds?.Count ?? 0}");

        Synced?.Invoke(snapshot);
    }

    private void ApplyQuests(ProfileSnapshot snapshot)
    {
        if (runtime.Quests == null || dataRepository == null)
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

            string id = quest.Definition.id;
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
        if (ids == null || dataRepository == null || runtime.Quests == null)
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

            QuestDefinitionData def = dataRepository.GetQuestById(id);
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

            string id = building.Definition.id;
            if (snapshot.BuildingStates != null && snapshot.BuildingStates.Count > 0)
            {
                var stateSnapshot = snapshot.BuildingStates.Find(s => s != null && s.id == id);
                if (stateSnapshot != null)
                {
                    building.IsOwned = stateSnapshot.owned;
                    building.Level = stateSnapshot.level;
                    building.CurrentIncome = stateSnapshot.currentIncome;
                    building.CurrentExpenses = stateSnapshot.currentExpenses;
                    continue;
                }
            }

            building.IsOwned = ownedSet.Contains(id);
        }
    }

    private static QuestState FindQuestState(string questId, List<QuestState> quests)
    {
        if (quests == null) return null;

        foreach (QuestState quest in quests)
        {
            if (quest != null && quest.Definition != null && quest.Definition.id == questId)
            {
                return quest;
            }
        }

        return null;
    }
}
