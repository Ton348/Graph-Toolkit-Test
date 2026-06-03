using System.Collections;
using System.Collections.Generic;
using Graph.Core.Runtime;
using Prototype.Business.Bootstrap;
using Prototype.Business.NPC.AI;
using Sample.Runtime.Services;
using Sample.Runtime.UI;
using Prototype.Business.Time;
using UnityEngine;

namespace Prototype.Business.NPC.Spawning
{
	public sealed class NPCSpawnManager : MonoBehaviour
	{
		[SerializeField] private List<GameObject> npcPrefabs = new();
		[SerializeField] private Transform player;
		[SerializeField] private int maxAlive = 20;
		[SerializeField] private float spawnIntervalSeconds = 3f;
		[SerializeField] private float despawnDelaySeconds = 2f;
		[SerializeField] private GameBootstrap bootstrap;
		[SerializeField] private DialogueService dialogueService;
		[SerializeField] private ChoiceUiservice choiceUiService;
		[SerializeField] private TradeOfferUiservice tradeOfferUiService;
		[SerializeField] private MapMarkerService mapMarkerService;
		[SerializeField] private CommonGraph behaviorGraph;
		[SerializeField] private CommonGraph dialogueGraph;
		[SerializeField] private CommonGraph dangerGraph;
		[SerializeField] private CommonGraph combatGraph;

		private readonly List<NPCBrain> m_alive = new();
		private NPCSpawnPoint[] m_spawnPoints;
		private NPCDespawnPoint[] m_despawnPoints;
		private Camera m_mainCamera;
		private float m_timer;
		private bool m_canSpawnByPhase = true;
		private int m_nextSpawnPointIndex;

		private void Awake()
		{
			m_mainCamera = Camera.main;
			m_spawnPoints = FindObjectsByType<NPCSpawnPoint>(FindObjectsSortMode.None);
			m_despawnPoints = FindObjectsByType<NPCDespawnPoint>(FindObjectsSortMode.None);
		}

		private void OnEnable()
		{
			if (WorldTimeSystem.Instance != null)
			{
				WorldTimeSystem.Instance.OnPhaseChanged += OnPhaseChanged;
				m_canSpawnByPhase = !WorldTimeSystem.Instance.IsNight();
			}
		}

		private void OnDisable()
		{
			if (WorldTimeSystem.Instance != null)
			{
				WorldTimeSystem.Instance.OnPhaseChanged -= OnPhaseChanged;
			}
		}

		private void Update()
		{
			CleanupDead();
			if (!m_canSpawnByPhase)
			{
				return;
			}

			m_timer += UnityEngine.Time.deltaTime;
			if (m_timer >= Mathf.Max(0.5f, spawnIntervalSeconds))
			{
				m_timer = 0f;
				TrySpawn();
			}
		}

		public void RequestDespawn(NPCBrain brain)
		{
			if (brain == null || !m_alive.Contains(brain))
			{
				return;
			}

			StartCoroutine(DespawnRoutine(brain));
		}

		private void TrySpawn()
		{
			GameObject prefabToSpawn = GetRandomPrefab();
			if (prefabToSpawn == null || m_spawnPoints == null || m_spawnPoints.Length == 0 || m_alive.Count >= maxAlive)
			{
				return;
			}

			int startIndex = m_nextSpawnPointIndex;
			for (int i = 0; i < m_spawnPoints.Length; i++)
			{
				int idx = (startIndex + i) % m_spawnPoints.Length;
				NPCSpawnPoint point = m_spawnPoints[idx];
				if (point == null || !point.CanSpawn(player, m_mainCamera))
				{
					continue;
				}

				GameObject go = Instantiate(prefabToSpawn, point.transform.position, point.transform.rotation);
				Npcmanager npcManager = go.GetComponent<Npcmanager>();
				if (npcManager != null)
				{
					npcManager.Initialize(
						bootstrap,
						dialogueService,
						choiceUiService,
						tradeOfferUiService,
						mapMarkerService,
						player,
						behaviorGraph,
						dialogueGraph,
						dangerGraph,
						combatGraph);
				}
				NPCBrain brain = go.GetComponent<NPCBrain>();
				if (brain != null)
				{
					m_alive.Add(brain);
				}

				m_nextSpawnPointIndex = (idx + 1) % m_spawnPoints.Length;
				break;
			}
		}

		private GameObject GetRandomPrefab()
		{
			if (npcPrefabs != null)
			{
				int validCount = 0;
				for (int i = 0; i < npcPrefabs.Count; i++)
				{
					if (npcPrefabs[i] != null)
					{
						validCount++;
					}
				}

				if (validCount > 0)
				{
					int pick = UnityEngine.Random.Range(0, validCount);
					for (int i = 0; i < npcPrefabs.Count; i++)
					{
						GameObject candidate = npcPrefabs[i];
						if (candidate == null)
						{
							continue;
						}

						if (pick == 0)
						{
							return candidate;
						}

						pick--;
					}
				}
			}

			return null;
		}

		private IEnumerator DespawnRoutine(NPCBrain brain)
		{
			if (brain == null)
			{
				yield break;
			}

			brain.BeginDespawn(m_despawnPoints);
			yield return new WaitForSeconds(Mathf.Max(0.2f, despawnDelaySeconds));

			if (brain != null)
			{
				m_alive.Remove(brain);
				Destroy(brain.gameObject);
			}
		}

		private void CleanupDead()
		{
			for (int i = m_alive.Count - 1; i >= 0; i--)
			{
				if (m_alive[i] == null)
				{
					m_alive.RemoveAt(i);
				}
			}
		}

		private void OnPhaseChanged(DayPhase phase)
		{
			m_canSpawnByPhase = phase != DayPhase.Night;
		}
	}
}
