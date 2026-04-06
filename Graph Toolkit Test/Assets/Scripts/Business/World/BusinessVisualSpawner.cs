using System.Collections.Generic;
using UnityEngine;

public class BusinessVisualSpawner : MonoBehaviour
{
    [SerializeField] private GameBootstrap bootstrap;
    [SerializeField] private BusinessVisualRegistry registry;

    private readonly Dictionary<string, GameObject> spawnedBySiteId = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, string> spawnedVisualIdsBySiteId = new Dictionary<string, string>();
    private readonly Dictionary<string, BusinessWorldRuntime> anchorsBySiteId = new Dictionary<string, BusinessWorldRuntime>();
    private PlayerStateSync subscribedStateSync;

    public void Initialize(GameBootstrap bootstrap, BusinessVisualRegistry registry)
    {
        Unsubscribe();
        this.bootstrap = bootstrap;
        this.registry = registry;
        Subscribe();
        RefreshFromState();
    }

    private void OnEnable()
    {
        Subscribe();
        RefreshFromState();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
        ClearAllSpawned();
    }

    private void Subscribe()
    {
        var stateSync = bootstrap != null ? bootstrap.PlayerStateSync : null;
        if (stateSync == null || subscribedStateSync == stateSync)
        {
            return;
        }

        subscribedStateSync = stateSync;
        subscribedStateSync.SnapshotApplied += OnSnapshotApplied;
    }

    private void Unsubscribe()
    {
        if (subscribedStateSync != null)
        {
            subscribedStateSync.SnapshotApplied -= OnSnapshotApplied;
            subscribedStateSync = null;
        }
    }

    private void OnSnapshotApplied(ProfileSnapshot snapshot)
    {
        RefreshFromState();
    }

    private void RefreshFromState()
    {
        if (bootstrap == null || registry == null || bootstrap.PlayerStateSync == null)
        {
            return;
        }

        RebuildAnchors();
        var desiredSites = new HashSet<string>();

        foreach (var site in bootstrap.PlayerStateSync.ConstructedSites.Values)
        {
            if (site == null || string.IsNullOrWhiteSpace(site.siteId))
            {
                continue;
            }

            desiredSites.Add(site.siteId);

            if (!site.isConstructed || string.IsNullOrWhiteSpace(site.visualId))
            {
                RemoveSpawned(site.siteId);
                continue;
            }

            EnsureSpawned(site);
        }

        var sitesToRemove = new List<string>();
        foreach (var pair in spawnedBySiteId)
        {
            if (!desiredSites.Contains(pair.Key))
            {
                sitesToRemove.Add(pair.Key);
            }
        }

        foreach (string siteId in sitesToRemove)
        {
            RemoveSpawned(siteId);
        }
    }

    private void RebuildAnchors()
    {
        anchorsBySiteId.Clear();
        var worldRuntimes = Object.FindObjectsByType<BusinessWorldRuntime>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var worldRuntime in worldRuntimes)
        {
            if (worldRuntime == null || string.IsNullOrWhiteSpace(worldRuntime.siteId))
            {
                continue;
            }

            string siteId = worldRuntime.siteId.Trim();
            if (!anchorsBySiteId.ContainsKey(siteId))
            {
                anchorsBySiteId.Add(siteId, worldRuntime);
            }
        }
    }

    private void EnsureSpawned(ConstructedSiteSnapshot site)
    {
        if (!anchorsBySiteId.TryGetValue(site.siteId, out BusinessWorldRuntime anchor) || anchor == null)
        {
            Debug.LogWarning($"[BusinessVisual] Missing scene anchor for siteId='{site.siteId}'.");
            RemoveSpawned(site.siteId);
            return;
        }

        GameObject prefab = registry.GetPrefab(site.visualId);
        if (prefab == null)
        {
            Debug.LogWarning($"[BusinessVisual] Missing prefab for visualId='{site.visualId}'.");
            RemoveSpawned(site.siteId);
            return;
        }

        if (spawnedBySiteId.TryGetValue(site.siteId, out GameObject existing) &&
            existing != null &&
            spawnedVisualIdsBySiteId.TryGetValue(site.siteId, out string existingVisualId) &&
            existingVisualId == site.visualId)
        {
            existing.transform.SetPositionAndRotation(anchor.transform.position, anchor.transform.rotation);
            if (existing.transform.parent != anchor.transform)
            {
                existing.transform.SetParent(anchor.transform, true);
            }
            return;
        }

        RemoveSpawned(site.siteId);

        GameObject instance = Instantiate(prefab, anchor.transform.position, anchor.transform.rotation, anchor.transform);
        instance.name = $"{prefab.name}_{site.siteId}";
        AssignBootstrap(instance);
        spawnedBySiteId[site.siteId] = instance;
        spawnedVisualIdsBySiteId[site.siteId] = site.visualId;
    }

    private void RemoveSpawned(string siteId)
    {
        if (string.IsNullOrWhiteSpace(siteId))
        {
            return;
        }

        if (spawnedBySiteId.TryGetValue(siteId, out GameObject instance) && instance != null)
        {
            Destroy(instance);
        }

        spawnedBySiteId.Remove(siteId);
        spawnedVisualIdsBySiteId.Remove(siteId);
    }

    private void ClearAllSpawned()
    {
        foreach (var pair in spawnedBySiteId)
        {
            if (pair.Value != null)
            {
                Destroy(pair.Value);
            }
        }

        spawnedBySiteId.Clear();
        spawnedVisualIdsBySiteId.Clear();
    }

    private void AssignBootstrap(GameObject instance)
    {
        if (instance == null || bootstrap == null)
        {
            return;
        }

        var buildingInteractables = instance.GetComponentsInChildren<BuildingInteractable>(true);
        if (buildingInteractables != null)
        {
            foreach (var interactable in buildingInteractables)
            {
                if (interactable != null)
                {
                    interactable.bootstrap = bootstrap;
                }
            }
        }

        var npcManagers = instance.GetComponentsInChildren<NPCManager>(true);
        if (npcManagers != null)
        {
            foreach (var npc in npcManagers)
            {
                if (npc != null)
                {
                    npc.bootstrap = bootstrap;
                }
            }
        }
    }
}
