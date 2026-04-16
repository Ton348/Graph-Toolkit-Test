using System;
using System.Collections.Generic;
using UnityEngine;

public class ProfileSyncService
{
	private readonly BusinessStateSyncService m_businessStateSync;
	private readonly GameDataRepository m_dataRepository;
	private readonly PlayerStateSync m_playerStateSync;
	private readonly GameRuntimeState m_runtime;

	public ProfileSyncService(
		GameRuntimeState runtime,
		GameDataRepository dataRepository,
		PlayerStateSync playerStateSync,
		BusinessStateSyncService businessStateSync)
	{
		m_runtime = runtime;
		m_dataRepository = dataRepository;
		m_playerStateSync = playerStateSync;
		m_businessStateSync = businessStateSync;
	}

	public event Action<ProfileSnapshot> synced;

	public void ApplySnapshot(ProfileSnapshot snapshot)
	{
		if (snapshot == null)
		{
			return;
		}

		m_playerStateSync?.ApplySnapshot(snapshot);
		m_businessStateSync?.ApplySnapshot(snapshot);

		Debug.Log(
			$"[ProfileSync] Applied snapshot: money={snapshot.money}, active={snapshot.activeQuestIds?.Count ?? 0}, completed={snapshot.completedQuestIds?.Count ?? 0}, owned={snapshot.ownedBuildingIds?.Count ?? 0}");

		synced?.Invoke(snapshot);
	}

	private void ApplyQuests(ProfileSnapshot snapshot)
	{
		if (m_runtime.quests == null || m_dataRepository == null)
		{
			return;
		}

		var activeSet = new HashSet<string>(snapshot.activeQuestIds ?? new List<string>());
		var completedSet = new HashSet<string>(snapshot.completedQuestIds ?? new List<string>());

		foreach (QuestState quest in m_runtime.quests)
		{
			if (quest == null || quest.definition == null)
			{
				continue;
			}

			string id = quest.definition.id;
			if (completedSet.Contains(id))
			{
				quest.status = QuestStatus.Completed;
			}
			else if (activeSet.Contains(id))
			{
				quest.status = QuestStatus.Active;
			}
			else
			{
				if (quest.status != QuestStatus.Failed)
				{
					quest.status = QuestStatus.Inactive;
				}
			}
		}

		EnsureQuestStates(activeSet, QuestStatus.Active);
		EnsureQuestStates(completedSet, QuestStatus.Completed);
	}

	private void EnsureQuestStates(HashSet<string> ids, QuestStatus status)
	{
		if (ids == null || m_dataRepository == null || m_runtime.quests == null)
		{
			return;
		}

		foreach (string id in ids)
		{
			if (string.IsNullOrEmpty(id))
			{
				continue;
			}

			QuestState existing = FindQuestState(id, m_runtime.quests);
			if (existing != null)
			{
				existing.status = status;
				continue;
			}

			QuestDefinitionData def = m_dataRepository.GetQuestById(id);
			if (def == null)
			{
				continue;
			}

			var state = new QuestState(def)
			{
				status = status
			};
			m_runtime.quests.Add(state);
		}
	}

	private void ApplyBuildings(ProfileSnapshot snapshot)
	{
		if (m_runtime.buildings == null)
		{
			return;
		}

		var ownedSet = new HashSet<string>(snapshot.ownedBuildingIds ?? new List<string>());

		foreach (BuildingState building in m_runtime.buildings)
		{
			if (building == null || building.definition == null)
			{
				continue;
			}

			string id = building.definition.id;
			if (snapshot.buildingStates != null && snapshot.buildingStates.Count > 0)
			{
				BuildingStateSnapshot stateSnapshot = snapshot.buildingStates.Find(s => s != null && s.id == id);
				if (stateSnapshot != null)
				{
					building.isOwned = stateSnapshot.owned;
					building.level = stateSnapshot.level;
					building.currentIncome = stateSnapshot.currentIncome;
					building.currentExpenses = stateSnapshot.currentExpenses;
					continue;
				}
			}

			building.isOwned = ownedSet.Contains(id);
		}
	}

	private static QuestState FindQuestState(string questId, List<QuestState> quests)
	{
		if (quests == null)
		{
			return null;
		}

		foreach (QuestState quest in quests)
		{
			if (quest != null && quest.definition != null && quest.definition.id == questId)
			{
				return quest;
			}
		}

		return null;
	}
}