using Prototype.Business.Bootstrap;
using UnityEngine;

namespace Prototype.Business.Simulation
{
	public class BusinessSimulationTickRunner : MonoBehaviour
	{
		public GameBootstrap bootstrap;
		public float tickIntervalSeconds = 1f;
		public float timeScale = 1f;
		private BusinessSimulationClock m_clock;

		private BusinessSimulationService m_simulationService;

		private void Awake()
		{
			if (bootstrap == null)
			{
				bootstrap = FindObjectOfType<GameBootstrap>();
			}

			if (bootstrap != null)
			{
				m_simulationService = bootstrap.BusinessSimulationService;
			}

			m_clock = new BusinessSimulationClock
			{
				tickIntervalSeconds = tickIntervalSeconds
			};
		}

		private void Update()
		{
			if (m_simulationService == null || m_clock == null)
			{
				return;
			}

			m_clock.tickIntervalSeconds = tickIntervalSeconds;
			m_simulationService.TimeScale = timeScale;
			m_clock.Update(Time.deltaTime);
			while (m_clock.TryConsumeTick(out float delta))
			{
				m_simulationService.RunTick(delta);
			}
		}
	}
}