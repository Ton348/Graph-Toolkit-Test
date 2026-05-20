using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Prototype.Business.Bootstrap;
using Prototype.Business.Runtime;
using UnityEngine;

namespace Prototype.Business.Time
{
	public sealed class BusinessDaySystem : MonoBehaviour
	{
		[SerializeField] private GameBootstrap bootstrap;
		private bool m_running;
		public event Action DayApplied;

		private void OnEnable()
		{
			if (bootstrap == null)
			{
				bootstrap = FindFirstObjectByType<GameBootstrap>(FindObjectsInactive.Include);
			}

			if (WorldTimeSystem.Instance != null)
			{
				WorldTimeSystem.Instance.OnDayChanged += OnDayChanged;
			}
		}

		private void OnDisable()
		{
			if (WorldTimeSystem.Instance != null)
			{
				WorldTimeSystem.Instance.OnDayChanged -= OnDayChanged;
			}
		}

		private void OnDayChanged(int day)
		{
			if (!isActiveAndEnabled || m_running)
			{
				return;
			}

			var runTask = RunDayTickAsync();
		}

		private async Task RunDayTickAsync()
		{
			if (bootstrap == null || bootstrap.BusinessActionFacade == null || bootstrap.BusinessRuntimeService == null)
			{
				return;
			}

			m_running = true;
			try
			{
				List<BusinessInstanceSnapshot> businesses = new();
				foreach (BusinessInstanceSnapshot b in bootstrap.BusinessRuntimeService.GetBusinesses())
				{
					if (b != null && !string.IsNullOrWhiteSpace(b.lotId))
					{
						businesses.Add(b);
					}
				}

				for (int i = 0; i < businesses.Count; i++)
				{
					await bootstrap.BusinessActionFacade.SimulateBusinessDay(businesses[i].lotId);
				}
			}
			finally
			{
				m_running = false;
				DayApplied?.Invoke();
			}
		}
	}
}
