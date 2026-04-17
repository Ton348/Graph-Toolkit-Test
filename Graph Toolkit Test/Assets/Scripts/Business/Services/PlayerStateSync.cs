using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Prototype.Business.Runtime;

namespace Prototype.Business.Services
{
	public class PlayerStateSync
	{
		private readonly HashSet<string> m_activeQuests = new();
		private readonly HashSet<string> m_completedQuests = new();
		private readonly Dictionary<string, ConstructedSiteSnapshot> m_constructedSites = new();
		private readonly Dictionary<string, string> m_graphCheckpoints = new();

		public int Money { get; private set; }
		public int Bargaining { get; private set; }
		public int Speech { get; private set; }
		public int Trading { get; private set; }
		public int Speed { get; private set; }
		public int Damage { get; private set; }
		public int Health { get; private set; }
		public IReadOnlyCollection<string> ActiveQuests => m_activeQuests;
		public IReadOnlyCollection<string> CompletedQuests => m_completedQuests;
		public IReadOnlyDictionary<string, string> GraphCheckpoints => m_graphCheckpoints;
		public IReadOnlyDictionary<string, ConstructedSiteSnapshot> ConstructedSites => m_constructedSites;

		public event Action<ProfileSnapshot> snapshotApplied;
		public event Action refreshRequested;

		public void ApplySnapshot(ProfileSnapshot snapshot)
		{
			if (snapshot == null)
			{
				return;
			}

			Money = snapshot.money;
			Bargaining = snapshot.bargaining;
			Speech = snapshot.speech;
			Trading = snapshot.trading;
			Speed = snapshot.speed;
			Damage = snapshot.damage;
			Health = snapshot.health;

			m_activeQuests.Clear();
			if (snapshot.activeQuestIds != null)
			{
				foreach (string id in snapshot.activeQuestIds)
				{
					if (!string.IsNullOrEmpty(id))
					{
						m_activeQuests.Add(id);
					}
				}
			}

			m_completedQuests.Clear();
			if (snapshot.completedQuestIds != null)
			{
				foreach (string id in snapshot.completedQuestIds)
				{
					if (!string.IsNullOrEmpty(id))
					{
						m_completedQuests.Add(id);
					}
				}
			}

			m_graphCheckpoints.Clear();
			if (snapshot.graphCheckpoints != null && snapshot.graphCheckpoints.Count > 0)
			{
				foreach (GraphCheckpointSnapshot checkpoint in snapshot.graphCheckpoints)
				{
					if (checkpoint == null || string.IsNullOrEmpty(checkpoint.graphId))
					{
						continue;
					}

					m_graphCheckpoints[checkpoint.graphId] = checkpoint.checkpointId;
				}
			}

			m_constructedSites.Clear();
			if (snapshot.constructedSites != null && snapshot.constructedSites.Count > 0)
			{
				foreach (ConstructedSiteSnapshot site in snapshot.constructedSites)
				{
					if (site == null || string.IsNullOrWhiteSpace(site.siteId))
					{
						continue;
					}

					string siteId = site.siteId.Trim();
					string visualId = string.IsNullOrWhiteSpace(site.visualId) ? null : site.visualId.Trim();
					bool isConstructed = site.isConstructed && !string.IsNullOrEmpty(visualId);
					m_constructedSites[siteId] = new ConstructedSiteSnapshot
					{
						siteId = siteId,
						visualId = isConstructed ? visualId : null,
						isConstructed = isConstructed
					};
				}
			}

			snapshotApplied?.Invoke(snapshot);
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
			m_activeQuests.Clear();
			m_completedQuests.Clear();
			m_graphCheckpoints.Clear();
			m_constructedSites.Clear();
			snapshotApplied?.Invoke(null);
		}

		public bool IsQuestActive(string questId)
		{
			return !string.IsNullOrEmpty(questId) && m_activeQuests.Contains(questId);
		}

		public bool IsQuestCompleted(string questId)
		{
			return !string.IsNullOrEmpty(questId) && m_completedQuests.Contains(questId);
		}

		public bool TryGetGraphCheckpoint(string graphId, out string checkpointId)
		{
			checkpointId = null;
			if (string.IsNullOrEmpty(graphId))
			{
				return false;
			}

			return m_graphCheckpoints.TryGetValue(graphId, out checkpointId);
		}

		public bool TryGetConstructedSite(string siteId, out ConstructedSiteSnapshot site)
		{
			site = null;
			if (string.IsNullOrWhiteSpace(siteId))
			{
				return false;
			}

			return m_constructedSites.TryGetValue(siteId.Trim(), out site);
		}

		public bool IsSiteConstructed(string siteId)
		{
			return TryGetConstructedSite(siteId, out ConstructedSiteSnapshot site) && site != null && site.isConstructed;
		}

		public UniTask RefreshAsync()
		{
			refreshRequested?.Invoke();
			return UniTask.CompletedTask;
		}

		public void Refresh()
		{
			refreshRequested?.Invoke();
		}

		public UniTask SyncAsync()
		{
			refreshRequested?.Invoke();
			return UniTask.CompletedTask;
		}
	}
}
