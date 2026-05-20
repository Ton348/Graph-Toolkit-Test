using System.Collections.Generic;
using Prototype.Business.Bootstrap;
using Prototype.Business.NPC.Spawning;
using Prototype.Business.Runtime;
using Prototype.Business.World;
using UnityEngine;

namespace Prototype.Business.NPC.Workers
{
	public sealed class WorkerNPCManager : MonoBehaviour
	{
		[SerializeField] private GameObject merchandiserPrefab;
		[SerializeField] private GameObject cashierPrefab;
		[SerializeField] private GameObject logistPrefab;
		[SerializeField] private float refreshSeconds = 1f;

		private readonly Dictionary<string, WorkerNPCBrain> m_merchByLotId = new();
		private readonly Dictionary<string, WorkerNPCBrain> m_cashierByLotId = new();
		private readonly Dictionary<string, WorkerNPCBrain> m_logistByLotId = new();
		private readonly Dictionary<string, int> m_logistSpawnedDayByLotId = new();
		private GameBootstrap m_bootstrap;
		private float m_timer;

		private void Awake()
		{
			m_bootstrap = FindFirstObjectByType<GameBootstrap>(FindObjectsInactive.Include);
		}

		private void Update()
		{
			m_timer += UnityEngine.Time.deltaTime;
			if (m_timer < Mathf.Max(0.2f, refreshSeconds))
			{
				return;
			}

			m_timer = 0f;
			RefreshWorkers();
		}

		private void RefreshWorkers()
		{
			if (m_bootstrap == null || m_bootstrap.BusinessStateSyncService == null)
			{
				return;
			}

			BusinessWorldRuntime[] worlds = FindObjectsByType<BusinessWorldRuntime>(FindObjectsSortMode.None);
			for (int i = 0; i < worlds.Length; i++)
			{
				BusinessWorldRuntime world = worlds[i];
				if (world == null || string.IsNullOrWhiteSpace(world.lotId))
				{
					continue;
				}

				BusinessInstanceSnapshot business = world.GetBusiness();
				if (business == null)
				{
					DespawnForLot(world.lotId);
					continue;
				}

				bool needMerch = !string.IsNullOrWhiteSpace(business.hiredMerchContactId) && business.isOpen;
				bool needCashier = !string.IsNullOrWhiteSpace(business.hiredCashierContactId) && business.isOpen;
				int currentDay = Mathf.Max(0, business.dayIndex);
				int lastSpawnedDay = -1;
				m_logistSpawnedDayByLotId.TryGetValue(world.lotId, out lastSpawnedDay);
				bool needLogist = !string.IsNullOrWhiteSpace(business.hiredLogistContactId) &&
				                  business.isOpen &&
				                  currentDay > lastSpawnedDay;
				EnsureWorker(world, WorkerNPCType.Merchandiser, needMerch);
				EnsureWorker(world, WorkerNPCType.Cashier, needCashier);
				EnsureWorker(world, WorkerNPCType.Logist, needLogist);
			}
		}

		private void EnsureWorker(BusinessWorldRuntime world, WorkerNPCType type, bool need)
		{
			Dictionary<string, WorkerNPCBrain> map = type == WorkerNPCType.Merchandiser
				? m_merchByLotId
				: type == WorkerNPCType.Cashier
					? m_cashierByLotId
					: m_logistByLotId;
			map.TryGetValue(world.lotId, out WorkerNPCBrain existing);

			if (!need)
			{
				if (existing != null)
				{
					Destroy(existing.gameObject, 1.5f);
				}
				map.Remove(world.lotId);
				return;
			}

			if (existing != null)
			{
				return;
			}

			GameObject prefab = type == WorkerNPCType.Merchandiser
				? merchandiserPrefab
				: type == WorkerNPCType.Cashier
					? cashierPrefab
					: logistPrefab;
			if (prefab == null)
			{
				return;
			}

			Vector3 spawnPos = ResolveSpawnPosition(world);
			GameObject go = Instantiate(prefab, spawnPos, Quaternion.identity);
			WorkerNPCBrain brain = go.GetComponent<WorkerNPCBrain>();
			if (brain == null)
			{
				brain = go.AddComponent<WorkerNPCBrain>();
			}
			if (go.GetComponent<WorkerNPCMovement>() == null)
			{
				go.AddComponent<WorkerNPCMovement>();
			}
			if (go.GetComponent<UnityEngine.AI.NavMeshAgent>() == null)
			{
				go.AddComponent<UnityEngine.AI.NavMeshAgent>();
			}

			WorkerNPCMovement movement = go.GetComponent<WorkerNPCMovement>();
			if (movement != null)
			{
				movement.WarpToNavMesh(spawnPos, 10f);
			}

			brain.Initialize(world, m_bootstrap, type);
			map[world.lotId] = brain;
			if (type == WorkerNPCType.Logist)
			{
				BusinessInstanceSnapshot business = world.GetBusiness();
				m_logistSpawnedDayByLotId[world.lotId] = business != null ? Mathf.Max(0, business.dayIndex) : 0;
			}
		}

		private Vector3 ResolveSpawnPosition(BusinessWorldRuntime world)
		{
			NPCSpawnPoint[] points = FindObjectsByType<NPCSpawnPoint>(FindObjectsSortMode.None);
			Transform player = ResolvePlayerTransform();
			Camera cam = Camera.main;
			Transform best = null;
			float bestDist = float.MaxValue;
			Vector3 worldPos = world != null ? world.transform.position : Vector3.zero;

			for (int i = 0; i < points.Length; i++)
			{
				NPCSpawnPoint p = points[i];
				if (p == null || !p.CanSpawn(player, cam))
				{
					continue;
				}

				float d = (p.transform.position - worldPos).sqrMagnitude;
				if (d < bestDist)
				{
					bestDist = d;
					best = p.transform;
				}
			}

			if (best != null)
			{
				return best.position;
			}

			if (world != null && world.EntrancePoint != null)
			{
				return world.EntrancePoint.position;
			}

			return world != null ? world.transform.position : Vector3.zero;
		}

		private Transform ResolvePlayerTransform()
		{
			NPCSpawnManager spawnManager = FindFirstObjectByType<NPCSpawnManager>(FindObjectsInactive.Include);
			if (spawnManager != null)
			{
				System.Reflection.FieldInfo playerField = typeof(NPCSpawnManager).GetField("player", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
				if (playerField != null)
				{
					Transform value = playerField.GetValue(spawnManager) as Transform;
					if (value != null)
					{
						return value;
					}
				}
			}

			GameObject go = GameObject.FindWithTag("Player");
			if (go != null)
			{
				return go.transform;
			}

			return null;
		}

		private void DespawnForLot(string lotId)
		{
			if (string.IsNullOrWhiteSpace(lotId))
			{
				return;
			}

			if (m_merchByLotId.TryGetValue(lotId, out WorkerNPCBrain merch) && merch != null)
			{
				Destroy(merch.gameObject, 1.5f);
			}
			m_merchByLotId.Remove(lotId);

			if (m_cashierByLotId.TryGetValue(lotId, out WorkerNPCBrain cashier) && cashier != null)
			{
				Destroy(cashier.gameObject, 1.5f);
			}
			m_cashierByLotId.Remove(lotId);

			if (m_logistByLotId.TryGetValue(lotId, out WorkerNPCBrain logist) && logist != null)
			{
				Destroy(logist.gameObject, 1.5f);
			}
			m_logistByLotId.Remove(lotId);
			m_logistSpawnedDayByLotId.Remove(lotId);
		}
	}
}
