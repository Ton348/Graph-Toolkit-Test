using System.Collections.Generic;
using UnityEngine;

public class BusinessVisualSpawner : MonoBehaviour
{
	[SerializeField]
	private GameBootstrap m_bootstrap;

	[SerializeField]
	private BusinessVisualRegistry m_registry;

	private readonly Dictionary<string, BusinessWorldRuntime> m_anchorsBySiteId = new();

	private readonly Dictionary<string, GameObject> m_spawnedBySiteId = new();
	private readonly Dictionary<string, string> m_spawnedVisualIdsBySiteId = new();
	private PlayerStateSync m_subscribedStateSync;

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

	public void Initialize(GameBootstrap bootstrap, BusinessVisualRegistry registry)
	{
		Unsubscribe();
		m_bootstrap = bootstrap;
		m_registry = registry;
		Subscribe();
		RefreshFromState();
	}

	private void Subscribe()
	{
		PlayerStateSync stateSync = m_bootstrap != null ? m_bootstrap.PlayerStateSync : null;
		if (stateSync == null || m_subscribedStateSync == stateSync)
		{
			return;
		}

		m_subscribedStateSync = stateSync;
		m_subscribedStateSync.snapshotApplied += OnSnapshotApplied;
	}

	private void Unsubscribe()
	{
		if (m_subscribedStateSync != null)
		{
			m_subscribedStateSync.snapshotApplied -= OnSnapshotApplied;
			m_subscribedStateSync = null;
		}
	}

	private void OnSnapshotApplied(ProfileSnapshot snapshot)
	{
		RefreshFromState();
	}

	private void RefreshFromState()
	{
		if (m_bootstrap == null || m_registry == null || m_bootstrap.PlayerStateSync == null)
		{
			return;
		}

		RebuildAnchors();
		var desiredSites = new HashSet<string>();

		foreach (ConstructedSiteSnapshot site in m_bootstrap.PlayerStateSync.ConstructedSites.Values)
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
		foreach (KeyValuePair<string, GameObject> pair in m_spawnedBySiteId)
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
		m_anchorsBySiteId.Clear();
		BusinessWorldRuntime[] worldRuntimes =
			FindObjectsByType<BusinessWorldRuntime>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		foreach (BusinessWorldRuntime worldRuntime in worldRuntimes)
		{
			if (worldRuntime == null || string.IsNullOrWhiteSpace(worldRuntime.siteId))
			{
				continue;
			}

			string siteId = worldRuntime.siteId.Trim();
			if (!m_anchorsBySiteId.ContainsKey(siteId))
			{
				m_anchorsBySiteId.Add(siteId, worldRuntime);
			}
		}
	}

	private void EnsureSpawned(ConstructedSiteSnapshot site)
	{
		if (!m_anchorsBySiteId.TryGetValue(site.siteId, out BusinessWorldRuntime anchor) || anchor == null)
		{
			Debug.LogWarning($"[BusinessVisual] Missing scene anchor for siteId='{site.siteId}'.");
			RemoveSpawned(site.siteId);
			return;
		}

		GameObject prefab = m_registry.GetPrefab(site.visualId);
		if (prefab == null)
		{
			Debug.LogWarning($"[BusinessVisual] Missing prefab for visualId='{site.visualId}'.");
			RemoveSpawned(site.siteId);
			return;
		}

		if (m_spawnedBySiteId.TryGetValue(site.siteId, out GameObject existing) &&
		    existing != null &&
		    m_spawnedVisualIdsBySiteId.TryGetValue(site.siteId, out string existingVisualId) &&
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

		GameObject instance =
			Instantiate(prefab, anchor.transform.position, anchor.transform.rotation, anchor.transform);
		instance.name = $"{prefab.name}_{site.siteId}";
		AssignBootstrap(instance);
		m_spawnedBySiteId[site.siteId] = instance;
		m_spawnedVisualIdsBySiteId[site.siteId] = site.visualId;
	}

	private void RemoveSpawned(string siteId)
	{
		if (string.IsNullOrWhiteSpace(siteId))
		{
			return;
		}

		if (m_spawnedBySiteId.TryGetValue(siteId, out GameObject instance) && instance != null)
		{
			Destroy(instance);
		}

		m_spawnedBySiteId.Remove(siteId);
		m_spawnedVisualIdsBySiteId.Remove(siteId);
	}

	private void ClearAllSpawned()
	{
		foreach (KeyValuePair<string, GameObject> pair in m_spawnedBySiteId)
		{
			if (pair.Value != null)
			{
				Destroy(pair.Value);
			}
		}

		m_spawnedBySiteId.Clear();
		m_spawnedVisualIdsBySiteId.Clear();
	}

	private void AssignBootstrap(GameObject instance)
	{
		if (instance == null || m_bootstrap == null)
		{
			return;
		}

		BuildingInteractable[] buildingInteractables = instance.GetComponentsInChildren<BuildingInteractable>(true);
		if (buildingInteractables != null)
		{
			foreach (BuildingInteractable interactable in buildingInteractables)
			{
				if (interactable != null)
				{
					interactable.bootstrap = m_bootstrap;
				}
			}
		}

		Npcmanager[] npcManagers = instance.GetComponentsInChildren<Npcmanager>(true);
		if (npcManagers != null)
		{
			foreach (Npcmanager npc in npcManagers)
			{
				if (npc != null)
				{
					npc.bootstrap = m_bootstrap;
				}
			}
		}
	}
}