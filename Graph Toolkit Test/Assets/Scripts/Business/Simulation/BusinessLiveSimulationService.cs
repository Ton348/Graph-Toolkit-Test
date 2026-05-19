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
			m_runtimes.Clear();
		}

		private void EvaluateAll()
		{
		}
	}
}
