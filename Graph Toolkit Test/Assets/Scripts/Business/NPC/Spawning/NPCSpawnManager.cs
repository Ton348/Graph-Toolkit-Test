using System.Collections;
using System.Collections.Generic;
using Prototype.Business.NPC.AI;
using UnityEngine;

namespace Prototype.Business.NPC.Spawning
{
	public sealed class NPCSpawnManager : MonoBehaviour
	{
		[SerializeField] private GameObject npcPrefab;
		[SerializeField] private Transform player;
		[SerializeField] private int maxAlive = 20;
		[SerializeField] private float spawnIntervalSeconds = 3f;
		[SerializeField] private float despawnDelaySeconds = 2f;

		private readonly List<NPCBrain> m_alive = new();
		private NPCSpawnPoint[] m_spawnPoints;
		private NPCDespawnPoint[] m_despawnPoints;
		private Camera m_mainCamera;
		private float m_timer;

		private void Awake()
		{
			m_mainCamera = Camera.main;
			m_spawnPoints = FindObjectsByType<NPCSpawnPoint>(FindObjectsSortMode.None);
			m_despawnPoints = FindObjectsByType<NPCDespawnPoint>(FindObjectsSortMode.None);
		}

		private void Update()
		{
			CleanupDead();
			m_timer += Time.deltaTime;
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
			if (npcPrefab == null || m_spawnPoints == null || m_spawnPoints.Length == 0 || m_alive.Count >= maxAlive)
			{
				return;
			}

			for (int i = 0; i < m_spawnPoints.Length; i++)
			{
				NPCSpawnPoint point = m_spawnPoints[i];
				if (point == null || !point.CanSpawn(player, m_mainCamera))
				{
					continue;
				}

				GameObject go = Instantiate(npcPrefab, point.transform.position, point.transform.rotation);
				NPCBrain brain = go.GetComponent<NPCBrain>();
				if (brain != null)
				{
					m_alive.Add(brain);
				}

				break;
			}
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
	}
}
