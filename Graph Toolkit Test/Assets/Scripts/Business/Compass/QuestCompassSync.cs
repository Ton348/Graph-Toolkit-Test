using System.Collections.Generic;
using UnityEngine;

public sealed class QuestCompassSync
{
    private readonly GameDataRepository m_dataRepository;
    private readonly PlayerStateSync m_playerStateSync;
    private readonly HashSet<string> m_activeMarkerIds = new HashSet<string>();

    public QuestCompassSync(GameDataRepository dataRepository, PlayerStateSync playerStateSync)
    {
        this.m_dataRepository = dataRepository;
        this.m_playerStateSync = playerStateSync;

        if (playerStateSync != null)
        {
            playerStateSync.snapshotApplied += OnSnapshotApplied;
        }
    }

    public void Refresh()
    {
        if (m_dataRepository == null || m_playerStateSync == null)
        {
            return;
        }

        var compass = CompassManager.Instance;
        if (compass == null)
        {
            return;
        }

        var desired = new HashSet<string>();

        foreach (var questId in m_playerStateSync.ActiveQuests)
        {
            if (string.IsNullOrEmpty(questId))
            {
                continue;
            }

            var quest = m_dataRepository.GetQuestById(questId);
            if (quest == null || string.IsNullOrEmpty(quest.markerId))
            {
                continue;
            }

            desired.Add(quest.markerId);
            if (!m_activeMarkerIds.Contains(quest.markerId))
            {
                compass.ShowTarget(quest.markerId);
            }
        }

        foreach (var markerId in m_activeMarkerIds)
        {
            if (!desired.Contains(markerId))
            {
                compass.HideTarget(markerId);
            }
        }

        m_activeMarkerIds.Clear();
        foreach (var markerId in desired)
        {
            m_activeMarkerIds.Add(markerId);
        }
    }

    private void OnSnapshotApplied(ProfileSnapshot snapshot)
    {
        Refresh();
    }
}
