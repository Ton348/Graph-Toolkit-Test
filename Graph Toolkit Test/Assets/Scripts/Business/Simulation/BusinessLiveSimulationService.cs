using System.Collections.Generic;
using Prototype.Business.Bootstrap;
using Prototype.Business.Runtime;
using Prototype.Business.World;
using UnityEngine;

namespace Prototype.Business.Simulation
{
	public class BusinessLiveSimulationService : MonoBehaviour
	{
		private readonly Dictionary<BusinessWorldRuntime, BusinessWorkerSplineRuntime> m_runtimes = new();
		private GameBootstrap m_bootstrap;
		private BusinessSimulationService m_simulation;
		private BusinessStateSyncService m_stateSync;

		private void OnDestroy()
		{
			if (m_stateSync != null)
			{
				m_stateSync.stateChanged -= OnStateChanged;
			}
		}

		public void Initialize(GameBootstrap ownerBootstrap)
		{
			m_bootstrap = ownerBootstrap;
			m_stateSync = m_bootstrap != null ? m_bootstrap.BusinessStateSyncService : null;
			m_simulation = m_bootstrap != null ? m_bootstrap.BusinessSimulationService : null;

			if (m_stateSync != null)
			{
				m_stateSync.stateChanged -= OnStateChanged;
				m_stateSync.stateChanged += OnStateChanged;
			}

			RebuildWorldRuntimes();
			EvaluateAll();
		}

		private void OnStateChanged()
		{
			RebuildWorldRuntimes();
			EvaluateAll();
		}

		private void RebuildWorldRuntimes()
		{
			BusinessWorldRuntime[] worlds = FindObjectsByType<BusinessWorldRuntime>(FindObjectsSortMode.None);
			var existing = new HashSet<BusinessWorldRuntime>(worlds);

			foreach (BusinessWorldRuntime world in worlds)
			{
				if (world == null || m_runtimes.ContainsKey(world))
				{
					continue;
				}

				var runtime = world.GetComponent<BusinessWorkerSplineRuntime>();
				if (runtime == null)
				{
					runtime = world.gameObject.AddComponent<BusinessWorkerSplineRuntime>();
				}

				runtime.worldRuntime = world;
				runtime.Initialize(m_simulation);
				m_runtimes[world] = runtime;
			}

			var toRemove = new List<BusinessWorldRuntime>();
			foreach (KeyValuePair<BusinessWorldRuntime, BusinessWorkerSplineRuntime> pair in m_runtimes)
			{
				if (pair.Key == null || !existing.Contains(pair.Key))
				{
					toRemove.Add(pair.Key);
				}
			}

			foreach (BusinessWorldRuntime key in toRemove)
			{
				m_runtimes.Remove(key);
			}
		}

		private void EvaluateAll()
		{
			foreach (BusinessWorkerSplineRuntime runtime in m_runtimes.Values)
			{
				if (runtime != null)
				{
					runtime.EvaluateActivation();
				}
			}
		}
	}
}