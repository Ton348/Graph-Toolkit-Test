using UnityEngine;

namespace Prototype.Business.Simulation
{
	public class BusinessSimulationClock
	{
		private float m_accumulator;
		public float tickIntervalSeconds = 1f;

		public void Update(float deltaTime)
		{
			if (tickIntervalSeconds <= 0f)
			{
				return;
			}

			m_accumulator += Mathf.Max(0f, deltaTime);
		}

		public bool TryConsumeTick(out float delta)
		{
			delta = 0f;
			if (tickIntervalSeconds <= 0f || m_accumulator < tickIntervalSeconds)
			{
				return false;
			}

			m_accumulator -= tickIntervalSeconds;
			delta = tickIntervalSeconds;
			return true;
		}
	}
}