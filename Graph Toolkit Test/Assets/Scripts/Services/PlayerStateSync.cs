using System;
using System.Collections.Generic;

public class PlayerStateSync
{
    private readonly HashSet<string> activeQuests = new HashSet<string>();
    private readonly HashSet<string> completedQuests = new HashSet<string>();
    private readonly HashSet<string> ownedBuildings = new HashSet<string>();
    private readonly Dictionary<string, BuildingStateSnapshot> buildingStates = new Dictionary<string, BuildingStateSnapshot>();
    private readonly Dictionary<string, string> graphCheckpoints = new Dictionary<string, string>();
    private readonly Dictionary<string, ConstructedSiteSnapshot> constructedSites = new Dictionary<string, ConstructedSiteSnapshot>();

    public int Money { get; private set; }
    public int Bargaining { get; private set; }
    public int Speech { get; private set; }
    public int Trading { get; private set; }
    public int Speed { get; private set; }
    public int Damage { get; private set; }
    public int Health { get; private set; }
    public IReadOnlyCollection<string> ActiveQuests => activeQuests;
    public IReadOnlyCollection<string> CompletedQuests => completedQuests;
    public IReadOnlyCollection<string> OwnedBuildings => ownedBuildings;
    public IReadOnlyDictionary<string, BuildingStateSnapshot> BuildingStates => buildingStates;
    public IReadOnlyDictionary<string, string> GraphCheckpoints => graphCheckpoints;
    public IReadOnlyDictionary<string, ConstructedSiteSnapshot> ConstructedSites => constructedSites;

    public event Action<ProfileSnapshot> SnapshotApplied;

    public void ApplySnapshot(ProfileSnapshot snapshot)
    {
        if (snapshot == null)
        {
            return;
        }

        Money = snapshot.Money;
        Bargaining = snapshot.Bargaining;
        Speech = snapshot.Speech;
        Trading = snapshot.Trading;
        Speed = snapshot.Speed;
        Damage = snapshot.Damage;
        Health = snapshot.Health;

        activeQuests.Clear();
        if (snapshot.ActiveQuestIds != null)
        {
            foreach (var id in snapshot.ActiveQuestIds)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    activeQuests.Add(id);
                }
            }
        }

        completedQuests.Clear();
        if (snapshot.CompletedQuestIds != null)
        {
            foreach (var id in snapshot.CompletedQuestIds)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    completedQuests.Add(id);
                }
            }
        }

        ownedBuildings.Clear();
        if (snapshot.OwnedBuildingIds != null)
        {
            foreach (var id in snapshot.OwnedBuildingIds)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    ownedBuildings.Add(id);
                }
            }
        }

        buildingStates.Clear();
        if (snapshot.BuildingStates != null && snapshot.BuildingStates.Count > 0)
        {
            foreach (var state in snapshot.BuildingStates)
            {
                if (state == null || string.IsNullOrEmpty(state.id))
                {
                    continue;
                }

                buildingStates[state.id] = state;
                if (state.owned)
                {
                    ownedBuildings.Add(state.id);
                }
            }
        }

        graphCheckpoints.Clear();
        if (snapshot.GraphCheckpoints != null && snapshot.GraphCheckpoints.Count > 0)
        {
            foreach (var checkpoint in snapshot.GraphCheckpoints)
            {
                if (checkpoint == null || string.IsNullOrEmpty(checkpoint.graphId))
                {
                    continue;
                }

                graphCheckpoints[checkpoint.graphId] = checkpoint.checkpointId;
            }
        }

        constructedSites.Clear();
        if (snapshot.ConstructedSites != null && snapshot.ConstructedSites.Count > 0)
        {
            foreach (var site in snapshot.ConstructedSites)
            {
                if (site == null || string.IsNullOrWhiteSpace(site.siteId))
                {
                    continue;
                }

                var siteId = site.siteId.Trim();
                var visualId = string.IsNullOrWhiteSpace(site.visualId) ? null : site.visualId.Trim();
                bool isConstructed = site.isConstructed && !string.IsNullOrEmpty(visualId);
                constructedSites[siteId] = new ConstructedSiteSnapshot
                {
                    siteId = siteId,
                    visualId = isConstructed ? visualId : null,
                    isConstructed = isConstructed
                };
            }
        }

        SnapshotApplied?.Invoke(snapshot);
    }

    public void Reset()
    {
        Money = 0;
        Bargaining = 0;
        Speech = 0;
        Trading = 0;
        Speed = 0;
        Damage = 0;
        Health = 0;
        activeQuests.Clear();
        completedQuests.Clear();
        ownedBuildings.Clear();
        buildingStates.Clear();
        graphCheckpoints.Clear();
        constructedSites.Clear();
        SnapshotApplied?.Invoke(null);
    }

    public bool IsQuestActive(string questId)
    {
        return !string.IsNullOrEmpty(questId) && activeQuests.Contains(questId);
    }

    public bool IsQuestCompleted(string questId)
    {
        return !string.IsNullOrEmpty(questId) && completedQuests.Contains(questId);
    }

    public bool IsBuildingOwned(string buildingId)
    {
        return !string.IsNullOrEmpty(buildingId) && ownedBuildings.Contains(buildingId);
    }

    public bool TryGetBuildingState(string buildingId, out BuildingStateSnapshot state)
    {
        if (string.IsNullOrEmpty(buildingId))
        {
            state = null;
            return false;
        }

        return buildingStates.TryGetValue(buildingId, out state);
    }

    public bool TryGetGraphCheckpoint(string graphId, out string checkpointId)
    {
        checkpointId = null;
        if (string.IsNullOrEmpty(graphId))
        {
            return false;
        }

        return graphCheckpoints.TryGetValue(graphId, out checkpointId);
    }

    public bool TryGetConstructedSite(string siteId, out ConstructedSiteSnapshot site)
    {
        site = null;
        if (string.IsNullOrWhiteSpace(siteId))
        {
            return false;
        }

        return constructedSites.TryGetValue(siteId.Trim(), out site);
    }

    public bool IsSiteConstructed(string siteId)
    {
        return TryGetConstructedSite(siteId, out var site) && site != null && site.isConstructed;
    }
}
