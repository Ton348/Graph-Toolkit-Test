using System.Collections.Generic;
using UnityEngine;

public sealed class QuestCompassSync
{
    private readonly GameDataRepository dataRepository;
    private readonly PlayerStateSync playerStateSync;
    private readonly HashSet<string> activeMarkerIds = new HashSet<string>();

    public QuestCompassSync(GameDataRepository dataRepository, PlayerStateSync playerStateSync)
    {
        this.dataRepository = dataRepository;
        this.playerStateSync = playerStateSync;

        if (playerStateSync != null)
        {
            playerStateSync.SnapshotApplied += OnSnapshotApplied;
        }
    }

    public void Refresh()
    {
        if (dataRepository == null || playerStateSync == null)
        {
            return;
        }

        var compass = CompassManager.Instance;
        if (compass == null)
        {
            return;
        }

        var desired = new HashSet<string>();

        foreach (var questId in playerStateSync.ActiveQuests)
        {
            if (string.IsNullOrEmpty(questId))
            {
                continue;
            }

            var quest = dataRepository.GetQuestById(questId);
            if (quest == null || string.IsNullOrEmpty(quest.markerId))
            {
                continue;
            }

            desired.Add(quest.markerId);
            if (!activeMarkerIds.Contains(quest.markerId))
            {
                compass.ShowTarget(quest.markerId);
            }
        }

        foreach (var markerId in activeMarkerIds)
        {
            if (!desired.Contains(markerId))
            {
                compass.HideTarget(markerId);
            }
        }

        activeMarkerIds.Clear();
        foreach (var markerId in desired)
        {
            activeMarkerIds.Add(markerId);
        }
    }

    private void OnSnapshotApplied(ProfileSnapshot snapshot)
    {
        Refresh();
    }
}
